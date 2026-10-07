using System;
using System.Collections.Generic;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using ZwcadPlugin.UI.Views;
using Newtonsoft.Json.Linq;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ZwcadPlugin.BlockConvertCommand))]

namespace ZwcadPlugin
{
    /// <summary>
    /// Cambia el tipo (3D / 3DRef / Xr) de los bloques Tandem seleccionados.
    /// Conserva punto, rotación y escala; borra e inserta el DWG destino.
    /// </summary>
    public class BlockConvertCommand
    {
        public const string CommandName = "TANDEM_CAMBIARBLOQUE";

        public static void QueueFromPalette(JObject obj)
        {
        }

        [CommandMethod(CommandName, CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void Run()
        {

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;

            TandemMiniPopup pop = null;
            try
            {
                pop = TandemMiniPopup.ShowTop("Cambiar tipo");
                pop.ShowPrompt("Selecciona los artículos a cambiar y pulsa la tecla Intro");
                BlockInsertCommand.FocusDrawing();

                var ids = GatherSelection(ed);
                if (pop.IsCancelled)
                    return;
                if (ids == null || ids.Count == 0)
                {
                    ed.WriteMessage("\n[Tandem] Selecciona artículos Tandem e Intro.\n");
                    return;
                }

                var msg = ids.Count == 1
                    ? "1 artículo. Elige el tipo:"
                    : ids.Count + " artículos. Elige el tipo:";
                pop.ShowChoices(msg, new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("3d", "3D"),
                    new KeyValuePair<string, string>("3dref", "3DRef"),
                    new KeyValuePair<string, string>("xr", "Xr")
                });
                var picked = pop.WaitForChoice();
                if (string.IsNullOrWhiteSpace(picked) || pop.IsCancelled)
                {
                    ed.WriteMessage("\n[Tandem] Cambio de tipo cancelado.\n");
                    return;
                }

                pop.ShowProgress("Cambiando bloques", "Leyendo bloques locales…");
                var target = BlockInsertCommand.NormalizeView(picked);
                int changed;
                int skipped;
                int failed;
                ConvertMany(doc, ed, ids, target, pop, out changed, out skipped, out failed);
                var targetLabel = target == "3d" ? "3D" : (target == "xr" ? "Xr" : "3DRef");
                ed.WriteMessage("\n[Tandem] Cambiados a " + targetLabel
                    + ": " + changed
                    + (skipped > 0 ? ", ya eran ese tipo: " + skipped : "")
                    + (failed > 0 ? ", sin DWG: " + failed : "")
                    + ".\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n[Tandem] No se pudo cambiar: " + ex.Message + "\n");
            }
            finally
            {
                if (pop != null)
                    pop.CloseSafe();
            }
        }

        private static List<ObjectId> GatherSelection(Editor ed)
        {
            var opts = new PromptSelectionOptions();
            opts.MessageForAdding = "\nSelecciona los artículos a cambiar (Intro para elegir tipo): ";
            opts.MessageForRemoval = "\nQuita artículos de la selección: ";
            var sel = ed.GetSelection(opts);

            var ids = new List<ObjectId>();
            if (sel.Status != PromptStatus.OK || sel.Value == null)
                return ids;

            var db = ed.Document.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in sel.Value)
                {
                    if (so == null) continue;
                    var br = tr.GetObject(so.ObjectId, OpenMode.ForRead) as BlockReference;
                    if (br == null) continue;
                    string code, view, role;
                    int rot;
                    if (BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rot))
                        ids.Add(so.ObjectId);
                }
                tr.Commit();
            }
            return ids;
        }

        private static void ConvertMany(
            Document doc,
            Editor ed,
            IList<ObjectId> ids,
            string target,
            TandemMiniPopup pop,
            out int changed,
            out int skipped,
            out int failed)
        {
            changed = 0;
            skipped = 0;
            failed = 0;
            var db = doc.Database;

            var jobs = new List<ConvertJob>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var id in ids)
                {
                    var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br == null) continue;
                    string code, view, role;
                    int rot;
                    if (!BlockInsertCommand.TryReadAtk(br, out code, out view, out role, out rot))
                        continue;
                    if (BlockInsertCommand.NormalizeView(view) == target)
                    {
                        skipped++;
                        continue;
                    }
                    jobs.Add(new ConvertJob
                    {
                        Id = id,
                        CodeName = code,
                        Role = role,
                        RotDeg = rot,
                        Transform = br.BlockTransform,
                        Layer = br.Layer
                    });
                }
                tr.Commit();
            }

            var defCache = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
            var n = 0;
            foreach (var job in jobs)
            {
                n++;
                if (pop != null)
                    pop.ShowProgress("Cambiando bloques", n + "/" + jobs.Count + " · " + job.CodeName + " → " + target);
                var blockName = BlockInsertCommand.BlockNameFor(job.CodeName, target);
                if (defCache.ContainsKey(blockName))
                    continue;
                var dwg = Atk60DwgResolver.ResolveDwg(job.CodeName, target);
                if (string.IsNullOrWhiteSpace(dwg))
                {
                    failed++;
                    job.Skip = true;
                    ed.WriteMessage("\n[Tandem] Sin DWG " + target + " para " + job.CodeName + ". Pulsa Actualizar en bloquing.");
                    continue;
                }
                try
                {
                    defCache[blockName] = BlockInsertCommand.EnsureBlockDefinition(doc, dwg, blockName);
                }
                catch (System.Exception ex)
                {
                    failed++;
                    job.Skip = true;
                    ed.WriteMessage("\n[Tandem] No se cargó " + blockName + ": " + ex.Message);
                }
            }

            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ms = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                foreach (var job in jobs)
                {
                    if (job.Skip)
                        continue;
                    ObjectId blockId;
                    if (!defCache.TryGetValue(BlockInsertCommand.BlockNameFor(job.CodeName, target), out blockId))
                        continue;

                    var old = (BlockReference)tr.GetObject(job.Id, OpenMode.ForWrite);
                    var neu = new BlockReference(Point3d.Origin, blockId);
                    neu.BlockTransform = job.Transform;
                    try { neu.Layer = job.Layer; } catch { }
                    ms.AppendEntity(neu);
                    tr.AddNewlyCreatedDBObject(neu, true);
                    BlockInsertCommand.ApplyAtkXData(neu, tr, db, job.CodeName, target, job.Role, job.RotDeg);
                    old.Erase();
                    changed++;
                }
                tr.Commit();
            }
        }

        private sealed class ConvertJob
        {
            public ObjectId Id;
            public string CodeName;
            public string Role;
            public int RotDeg;
            public Matrix3d Transform;
            public string Layer;
            public bool Skip;
        }
    }
}


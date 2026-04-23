using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwcadPlugin.Models;
using ZwcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;
using ZwSoft.ZwCAD.ApplicationServices;

namespace ZwcadPlugin
{
    public class FormPrincipal : Form
    {
        private readonly MVCApiService _apiService;

        // Controles de la pestaña Bloques
        private TabControl tabControl;
        private TabPage tabBloques;
        private TabPage tabDisenos;
        private ListBox lstBloques;
        private TextBox txtEscala;
        private TextBox txtRotacion;
        private Button btnCargarBloques;
        private Button btnInsertarBloque;
        private Label lblEscala;
        private Label lblRotacion;

        // Controles de la pestaña Diseños
        private ListBox lstDisenos;
        private TextBox txtNombreDiseno;
        private TextBox txtDescripcion;
        private Button btnCargarLista;
        private Button btnLeerServidor;
        private Button btnGuardarServidor;
        private Label lblNombre;
        private Label lblDescripcion;

        public FormPrincipal()
        {
            _apiService = new MVCApiService();
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            // Configuración del formulario
            this.Text = "Plugin ZWCAD 2026 - Conexión MVC";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(tabControl);

            // Crear pestañas
            CrearTabBloques();
            CrearTabDisenos();
        }

        #region Pestaña Bloques

        private void CrearTabBloques()
        {
            tabBloques = new TabPage("Bloques");
            tabControl.TabPages.Add(tabBloques);

            // ListBox de bloques
            lstBloques = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(540, 200),
                DisplayMember = "Nombre"
            };
            tabBloques.Controls.Add(lstBloques);

            // Label y TextBox de Escala
            lblEscala = new Label
            {
                Text = "Escala:",
                Location = new Point(20, 240),
                Size = new Size(100, 20)
            };
            tabBloques.Controls.Add(lblEscala);

            txtEscala = new TextBox
            {
                Location = new Point(130, 238),
                Size = new Size(100, 20),
                Text = "1.0"
            };
            tabBloques.Controls.Add(txtEscala);

            // Label y TextBox de Rotación
            lblRotacion = new Label
            {
                Text = "Rotación (grados):",
                Location = new Point(20, 270),
                Size = new Size(100, 20)
            };
            tabBloques.Controls.Add(lblRotacion);

            txtRotacion = new TextBox
            {
                Location = new Point(130, 268),
                Size = new Size(100, 20),
                Text = "0"
            };
            tabBloques.Controls.Add(txtRotacion);

            // Botón Cargar Bloques
            btnCargarBloques = new Button
            {
                Text = "Cargar Bloques del Servidor",
                Location = new Point(20, 310),
                Size = new Size(250, 40)
            };
            btnCargarBloques.Click += BtnCargarBloques_Click;
            tabBloques.Controls.Add(btnCargarBloques);

            // Botón Insertar Bloque
            btnInsertarBloque = new Button
            {
                Text = "Insertar Bloque Seleccionado",
                Location = new Point(310, 310),
                Size = new Size(250, 40)
            };
            btnInsertarBloque.Click += BtnInsertarBloque_Click;
            tabBloques.Controls.Add(btnInsertarBloque);
        }

        private async void BtnCargarBloques_Click(object sender, EventArgs e)
        {
            try
            {
                btnCargarBloques.Enabled = false;
                btnCargarBloques.Text = "Cargando...";

                var bloques = await _apiService.ObtenerBloquesAsync();
                lstBloques.DataSource = bloques;

                MessageBox.Show($"Se cargaron {bloques.Count} bloques del servidor.", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar bloques:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCargarBloques.Enabled = true;
                btnCargarBloques.Text = "Cargar Bloques del Servidor";
            }
        }

        private void BtnInsertarBloque_Click(object sender, EventArgs e)
        {
            if (lstBloques.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un bloque de la lista.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var bloqueSeleccionado = (BloqueDTO)lstBloques.SelectedItem;

            // Validar escala
            if (!double.TryParse(txtEscala.Text, out double escala))
            {
                MessageBox.Show("La escala debe ser un número válido.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validar rotación
            if (!double.TryParse(txtRotacion.Text, out double rotacion))
            {
                MessageBox.Show("La rotación debe ser un número válido.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Cerrar el formulario y pasar al modo de inserción
                this.Hide();
                InsertarBloqueEnDibujo(bloqueSeleccionado.Nombre, escala, rotacion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al insertar bloque:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close();
            }
        }

        private void InsertarBloqueEnDibujo(string nombreBloque, double escala, double rotacion)
        {
            Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Solicitar punto de inserción
            PromptPointOptions ppo = new PromptPointOptions("\nEspecifica punto de inserción: ");
            PromptPointResult ppr = ed.GetPoint(ppo);

            if (ppr.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                // Verificar si el bloque existe
                if (!bt.Has(nombreBloque))
                {
                    ed.WriteMessage($"\nError: El bloque '{nombreBloque}' no existe en el dibujo.");
                    ed.WriteMessage("\nPrimero debes cargar el bloque desde el servidor.");
                    return;
                }

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(
                    bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                // Crear referencia al bloque
                using (BlockReference br = new BlockReference(ppr.Value, bt[nombreBloque]))
                {
                    br.ScaleFactors = new Scale3d(escala, escala, escala);
                    br.Rotation = ZwcadHelper.GradosARadianes(rotacion);

                    btr.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    ed.WriteMessage($"\nBloque '{nombreBloque}' insertado correctamente.");
                }

                tr.Commit();
            }
        }

        #endregion

        #region Pestaña Diseños

        private void CrearTabDisenos()
        {
            tabDisenos = new TabPage("Diseños");
            tabControl.TabPages.Add(tabDisenos);

            // ListBox de diseños
            lstDisenos = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(540, 150),
                DisplayMember = "Nombre"
            };
            tabDisenos.Controls.Add(lstDisenos);

            // Label y TextBox de Nombre
            lblNombre = new Label
            {
                Text = "Nombre del Diseño:",
                Location = new Point(20, 190),
                Size = new Size(120, 20)
            };
            tabDisenos.Controls.Add(lblNombre);

            txtNombreDiseno = new TextBox
            {
                Location = new Point(150, 188),
                Size = new Size(410, 20)
            };
            tabDisenos.Controls.Add(txtNombreDiseno);

            // Label y TextBox de Descripción
            lblDescripcion = new Label
            {
                Text = "Descripción:",
                Location = new Point(20, 220),
                Size = new Size(120, 20)
            };
            tabDisenos.Controls.Add(lblDescripcion);

            txtDescripcion = new TextBox
            {
                Location = new Point(150, 218),
                Size = new Size(410, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            tabDisenos.Controls.Add(txtDescripcion);

            // Botones
            btnCargarLista = new Button
            {
                Text = "Cargar Lista",
                Location = new Point(20, 300),
                Size = new Size(170, 40)
            };
            btnCargarLista.Click += BtnCargarLista_Click;
            tabDisenos.Controls.Add(btnCargarLista);

            btnLeerServidor = new Button
            {
                Text = "Leer del Servidor",
                Location = new Point(205, 300),
                Size = new Size(170, 40)
            };
            btnLeerServidor.Click += BtnLeerServidor_Click;
            tabDisenos.Controls.Add(btnLeerServidor);

            btnGuardarServidor = new Button
            {
                Text = "Guardar en Servidor",
                Location = new Point(390, 300),
                Size = new Size(170, 40)
            };
            btnGuardarServidor.Click += BtnGuardarServidor_Click;
            tabDisenos.Controls.Add(btnGuardarServidor);
        }

        private async void BtnCargarLista_Click(object sender, EventArgs e)
        {
            try
            {
                btnCargarLista.Enabled = false;
                btnCargarLista.Text = "Cargando...";

                var disenos = await _apiService.ObtenerDisenosAsync();
                lstDisenos.DataSource = disenos;

                MessageBox.Show($"Se cargaron {disenos.Count} diseños del servidor.", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar diseños:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCargarLista.Enabled = true;
                btnCargarLista.Text = "Cargar Lista";
            }
        }

        private async void BtnLeerServidor_Click(object sender, EventArgs e)
        {
            if (lstDisenos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un diseño de la lista.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var disenoResumen = (DisenoResumenDTO)lstDisenos.SelectedItem;

            try
            {
                btnLeerServidor.Enabled = false;
                btnLeerServidor.Text = "Leyendo...";

                var diseno = await _apiService.ObtenerDisenoAsync(disenoResumen.Id);

                txtNombreDiseno.Text = diseno.Nombre;
                txtDescripcion.Text = diseno.Descripcion;

                string info = $"Diseño: {diseno.Nombre}\n";
                info += $"Descripción: {diseno.Descripcion}\n";
                info += $"Usuario: {diseno.Usuario}\n";
                info += $"Fecha: {diseno.FechaModificacion}\n";
                info += $"Entidades: {diseno.Entidades?.Count ?? 0}\n";
                info += $"Bloques: {diseno.Bloques?.Count ?? 0}\n";
                info += $"Layers: {diseno.Layers?.Count ?? 0}";

                MessageBox.Show(info, "Información del Diseño", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer diseño:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLeerServidor.Enabled = true;
                btnLeerServidor.Text = "Leer del Servidor";
            }
        }

        private async void BtnGuardarServidor_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreDiseno.Text))
            {
                MessageBox.Show("Ingresa un nombre para el diseño.", 
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGuardarServidor.Enabled = false;
                btnGuardarServidor.Text = "Guardando...";

                Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;

                // Extraer datos del dibujo
                var diseno = new DisenoDTO
                {
                    Nombre = txtNombreDiseno.Text,
                    Descripcion = txtDescripcion.Text,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    Usuario = ZwcadHelper.ObtenerUsuarioActual(),
                    Entidades = ZwcadHelper.ExtraerEntidades(db),
                    Bloques = ZwcadHelper.ExtraerBloques(db),
                    Layers = ZwcadHelper.ExtraerLayers(db)
                };

                // Enviar al servidor
                var disenoGuardado = await _apiService.CrearDisenoAsync(diseno);

                MessageBox.Show($"Diseño guardado exitosamente.\nID: {disenoGuardado.Id}", 
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar diseño:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardarServidor.Enabled = true;
                btnGuardarServidor.Text = "Guardar en Servidor";
            }
        }

        #endregion
    }
}

/**
 * One-off helper: strips unused legacy entities from Model.edmx (SSDL + CSDL + MSL).
 * Run: node edmx-prune-unused.mjs
 * Requires Node.js (no npm deps).
 */
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const edmxPath = path.join(__dirname, "Model.edmx");

/** Store / conceptual EntityType Name values to remove (exact SS DL EntityType Name). */
const REMOVE_ENTITY_TYPES = [
  "Maestro_articulos_temporal",
  "Movimientos",
  "perierase",
  "temporalmaestro",
  "TSql_Branch1",
  "TSql_Business",
  "TSql_Business1",
  "TSql_Client",
  "TSql_Comercial1",
  "TSql_language",
  "TSql_LanguageConcept",
  "TSql_LanguageDetails",
  "TSql_UserBranch",
  "tbDCulture",
  "sysdiagrams",
  "AminData",
  "Angel_01_Net_user",
  "Angel_02_Employee",
  "Angel_03_DefaulDesing",
  "Desing_details",
  "Empledos",
  "Employee",
  "Funciones de usuari",
  "MasterArticle",
  "Personal",
  "Q",
  "Register",
  "View_1",
  "View_2",
  "view_name",
];

function escapeRe(x) {
  return x.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function stripXmlBlocks(tagOpenRegex, s) {
  let prev;
  do {
    prev = s;
    s = s.replace(tagOpenRegex, "");
  } while (s !== prev);
  return s;
}

/** Diagram-related storage functions (SSDL). */
function stripDiagramFunctions(s) {
  return s.replace(
    /\s*<Function Name="fn_diagramobjects"[\s\S]*?<\/Function>/g,
    ""
  )
    .replace(/\s*<Function Name="sp_alterdiagram"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_creatediagram"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_dropdiagram"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_helpdiagramdefinition"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_helpdiagrams"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_renamediagram"[\s\S]*?<\/Function>/g, "")
    .replace(/\s*<Function Name="sp_upgraddiagrams"\/>/g, "")
    .replace(/\s*<Function Name="sp_upgraddiagrams"[^>]*\/>/g, "");
}

/** CSDL FunctionImport block under EntityContainer. */
function stripDiagramFunctionImports(s) {
  return s
    .replace(/\s*<FunctionImport Name="sp_alterdiagram"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_creatediagram"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_dropdiagram"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_helpdiagramdefinition"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_helpdiagrams"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_renamediagram"[\s\S]*?<\/FunctionImport>/g, "")
    .replace(/\s*<FunctionImport Name="sp_upgraddiagrams"\s*\/>/g, "");
}

/** CSDL ComplexType results for diagram sprocs. */
function stripDiagramComplexTypes(s) {
  return s
    .replace(
      /\s*<ComplexType Name="sp_helpdiagramdefinition_Result">[\s\S]*?<\/ComplexType>/g,
      ""
    )
    .replace(/\s*<ComplexType Name="sp_helpdiagrams_Result">[\s\S]*?<\/ComplexType>/g, "");
}

/** MSL FunctionImportMapping entries at end of Mapping. */
function stripDiagramFunctionImportMappings(s) {
  return s
    .replace(
      /\s*<FunctionImportMapping FunctionImportName="sp_alterdiagram"[\s\S]*?\/>/g,
      ""
    )
    .replace(/\s*<FunctionImportMapping FunctionImportName="sp_creatediagram"[\s\S]*?\/>/g, "")
    .replace(/\s*<FunctionImportMapping FunctionImportName="sp_dropdiagram"[\s\S]*?\/>/g, "")
    .replace(
      /\s*<FunctionImportMapping FunctionImportName="sp_helpdiagramdefinition"[\s\S]*?<\/FunctionImportMapping>/g,
      ""
    )
    .replace(
      /\s*<FunctionImportMapping FunctionImportName="sp_helpdiagrams"[\s\S]*?<\/FunctionImportMapping>/g,
      ""
    )
    .replace(/\s*<FunctionImportMapping FunctionImportName="sp_renamediagram"[\s\S]*?\/>/g, "")
    .replace(/\s*<FunctionImportMapping FunctionImportName="sp_upgraddiagrams"[\s\S]*?\/>/g, "");
}

let s = fs.readFileSync(edmxPath, "utf8");

for (const name of REMOVE_ENTITY_TYPES) {
  const en = escapeRe(name);
  s = stripXmlBlocks(new RegExp(`\\s*<EntityType Name="${en}">[\\s\\S]*?<\\/EntityType>`, "g"), s);
  s = stripXmlBlocks(
    new RegExp(`\\s*<EntitySet Name="${en}"[^>]*(?:\\/>|>[\\s\\S]*?<\\/EntitySet>)`, "g"),
    s
  );
  s = stripXmlBlocks(
    new RegExp(
      `\\s*<EntitySetMapping Name="${en}">[\\s\\S]*?<\\/EntitySetMapping>`,
      "g"
    ),
    s
  );
}

// Conceptual EntitySet uses Funciones_de_usuari while store uses "Funciones de usuari"
s = stripXmlBlocks(
  /\s*<EntitySet Name="Funciones_de_usuari"[^>]*(?:\/>|>[\s\S]*?<\/EntitySet>)/g,
  s
);
s = stripXmlBlocks(
  /\s*<EntityType Name="Funciones_de_usuari">[\s\S]*?<\/EntityType>/g,
  s
);
s = stripXmlBlocks(
  /\s*<EntitySetMapping Name="Funciones_de_usuari">[\s\S]*?<\/EntitySetMapping>/g,
  s
);

s = stripDiagramFunctions(s);
s = stripDiagramFunctionImports(s);
s = stripDiagramComplexTypes(s);
s = stripDiagramFunctionImportMappings(s);

fs.writeFileSync(edmxPath, s, "utf8");
console.log("Updated Model.edmx — stripped unused entities + DB diagram artifacts.");

/* One-off: verify Model.edmx.diagram AssociationConnector refs exist in CSDL. */
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const diagram = fs.readFileSync(path.join(__dirname, "Model.edmx.diagram"), "utf8");
const edmx = fs.readFileSync(path.join(__dirname, "Model.edmx"), "utf8");
const cs = edmx.indexOf("<edmx:ConceptualModels>");
const ce = edmx.indexOf("</edmx:ConceptualModels>");
const conceptual = edmx.slice(cs, ce);
const re = /Association="DesingEntity\.([^"]+)"/g;
const seen = new Set();
const missing = [];
let m;
while ((m = re.exec(diagram))) {
  const name = m[1];
  if (seen.has(name)) continue;
  seen.add(name);
  if (!conceptual.includes(`Association Name="${name}"`)) missing.push(name);
}
console.log(missing.length ? `Missing associations in CSDL (${missing.length}):\n${missing.join("\n")}` : "OK — all diagram associations found in CSDL.");

const reShape = /EntityType="DesingEntity\.([^"]+)"/g;
seen.clear();
const missEnt = [];
while ((m = reShape.exec(diagram))) {
  const name = m[1];
  if (seen.has(name)) continue;
  seen.add(name);
  if (!conceptual.includes(`EntityType Name="${name}"`)) missEnt.push(name);
}
console.log(missEnt.length ? `Missing entity types in CSDL (${missEnt.length}):\n${missEnt.join("\n")}` : "OK — all diagram entity types found in CSDL.");

import re
p = r"c:\00_Tandem2026\Desing\assets\materio\vendor\js\menu.js"
with open(p, "r", encoding="utf-8") as f:
    c = f.read()
out = []
out.append(f"len={len(c)}")
for pat in ["Toggable", "_getItem", "menu-toggle", "_evntElClick", "closest"]:
    out.append(f"{pat} idx={c.find(pat)}")
m = re.search(r'eval\("((?:\\.|[^"\\])*)"\)', c)
if m:
    src = m.group(1).encode("utf-8").decode("unicode_escape")
    lines = src.split("\n")
    out.append(f"source lines={len(lines)}")
    for i, l in enumerate(lines):
        if any(
            x in l
            for x in [
                "_getItem",
                "toggle",
                "menu-toggle",
                "Toggable",
                "_evntElClick",
                "closest",
            ]
        ):
            out.append(f"{i+1}: {l}")
with open(r"c:\00_Tandem2026\Desing\Scripts\_temp_parse_menu_out.txt", "w", encoding="utf-8") as f:
    f.write("\n".join(out))

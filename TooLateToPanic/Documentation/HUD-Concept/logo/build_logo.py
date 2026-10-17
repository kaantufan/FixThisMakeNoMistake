"""Too Late to Panic! logo generator.

All letterforms are hand-built SVG geometry (no font dependency), so every
output renders the same in a browser, Unity import pipeline or print.
Run:  python build_logo.py   (writes the SVG variants next to this file)
"""
import os

HERE = os.path.dirname(os.path.abspath(__file__))

CREAM, INK, DEEP = "#F6F1E8", "#292625", "#1A1817"
TEAL, AMBER, RUST, ALARM, STONE = "#256A6C", "#F2A93B", "#C65B3D", "#E4472D", "#D8D2C8"

# ---------------------------------------------------------------- PANIC glyphs
# Local units: cap height 140. Each entry: (x offset, path with evenodd counters).
HG_X = 344  # the hourglass replaces the letter I
GLYPHS = [
    (0,    "M0 0H60A42 42 0 0 1 60 84H34V140H0Z M34 24H58A18 18 0 0 1 58 60H34Z"),       # P
    (108,  "M0 140L40 0H74L114 140H80L73 112H41L34 140Z M47 88H67L57 50Z"),               # A
    (232,  "M0 140V0H32L68 84V0H100V140H68L32 56V140Z"),                                  # N
    # I = hourglass silhouette: caps + two bulbs pinched at the waist
    (HG_X, "M0 3A3 3 0 0 1 3 0H67A3 3 0 0 1 70 3V16H64C64 44 45 58 40 70C45 82 64 96 64 124H70V137A3 3 0 0 1 67 140H3A3 3 0 0 1 0 137V124H6C6 96 25 82 30 70C25 58 6 44 6 16H0Z"),
    (422,  "M104 43.5A54 70 0 1 0 104 96.5L74 88.3A22 44 0 1 1 74 51.7Z"),               # C
    (542,  "M0 0H36L29 100H7Z"),                                                          # ! bar
]
DROP = (542, "M18 106C25 116 32 123 32 129A14 14 0 0 1 4 129C4 123 11 116 18 106Z")  # ! dot as a drop

# Earthquake fault through the whole word; the lower half slides by SHIFT.
FAULT = [(-20, 60), (0, 62), (58, 68), (96, 61), (150, 78), (206, 71), (262, 86), (318, 80),
         (372, 95), (410, 89), (470, 99), (530, 93), (582, 104), (600, 106)]
SHIFT = (10, 9)
WORD_W = 578
WORD_SCALE = 1.42
WORD_POS = (round((1000 - WORD_W * WORD_SCALE) / 2), 226)


def pts(points):
    return " ".join(f"{x:g},{y:g}" for x, y in points)


def glyph_paths(attrs=""):
    return "".join(f'<path transform="translate({x} 0)" d="{d}" fill-rule="evenodd"{attrs}/>' for x, d in GLYPHS)


def hourglass_glass(edge, sand, glass):
    """Glass window and sand inside the hourglass glyph (local to HG_X)."""
    window = "M16 24H54C54 45 41 58 37 66V74C41 82 54 95 54 116H16C16 95 29 82 33 74V66C29 58 16 45 16 24Z"
    return (f'<g class="hg" transform="translate({HG_X} 0)">'
            f'<path d="{window}" fill="{glass}" stroke="{edge}" stroke-width="2" stroke-linejoin="round"/>'
            f'<path class="sand-top" d="M23 48H47C44 55 39 61 35 64C31 61 26 55 23 48Z" fill="{sand}"/>'
            f'<line class="sand-stream" x1="35" y1="64" x2="35" y2="104" stroke="{sand}" stroke-width="2.6" stroke-dasharray="4 3"/>'
            f'<path class="sand-pile" d="M18 116C21 102 30 96 35 95C40 96 49 102 52 116Z" fill="{sand}"/>'
            f'<path d="M20 30C20 41 25 49 30 56" fill="none" stroke="{edge}" stroke-width="1.6" stroke-linecap="round" opacity=".45"/></g>')


def top_row(fill, shadow, hands):
    """'TOO LATE' in heavy deco caps; the second O is a clock at 23:59."""
    letters = ("M0 0H48M24 0V66"                       # T
               "M255 0V66H295"                         # L
               "M317 66L343 0L369 66M327 44H359"       # A
               "M395 0H443M419 0V66"                   # T
               "M512 0H472V66H512M472 33H504")         # E
    shapes = f'<path d="{letters}"/><ellipse cx="99" cy="33" rx="22" ry="33"/><ellipse cx="179" cy="33" rx="22" ry="33"/>'
    group = lambda col: f'<g fill="none" stroke="{col}" stroke-width="14" stroke-linejoin="miter" stroke-miterlimit="6">{shapes}</g>'
    out = f'<g transform="translate(3.5 4.5)">{group(shadow)}</g>' if shadow else ""
    out += group(fill)
    out += (f'<g class="clock" stroke="{hands}" stroke-linecap="round">'
            f'<line class="hand-m" x1="179" y1="33" x2="175.5" y2="12" stroke-width="3.4"/>'
            f'<line x1="179" y1="33" x2="177" y2="21" stroke-width="4.2"/>'
            f'<circle cx="179" cy="33" r="3.4" fill="{hands}" stroke="none"/></g>')
    return out


def build_wordmark(v):
    uid = v["id"]
    up_poly = [(-30, -30), (620, -30), (620, 106)] + list(reversed(FAULT)) + [(-30, 60)]
    dn_poly = FAULT + [(620, 106), (620, 220), (-30, 220), (-30, 60)]
    sx, sy = SHIFT
    dx, dd = DROP

    defs = f'''<defs>
  <linearGradient id="{uid}-face" x1="0" y1="0" x2="0" y2="149" gradientUnits="userSpaceOnUse">
    <stop offset="0" stop-color="{v['face_top']}"/><stop offset=".55" stop-color="{v['face_mid']}"/><stop offset="1" stop-color="{v['face_bot']}"/>
  </linearGradient>
  <pattern id="{uid}-water" width="8" height="7" patternUnits="userSpaceOnUse"><rect width="8" height="2" fill="{v['water'] or 'none'}"/></pattern>
  <filter id="{uid}-glow" x="-10%" y="-60%" width="120%" height="220%"><feGaussianBlur stdDeviation="3"/></filter>
  <clipPath id="{uid}-up"><polygon points="{pts(up_poly)}"/></clipPath>
  <clipPath id="{uid}-dn"><polygon points="{pts(dn_poly)}"/></clipPath>
  <clipPath id="{uid}-letters">{glyph_paths()}</clipPath>
  <g id="{uid}-split">
    <g clip-path="url(#{uid}-up)">{glyph_paths()}</g>
    <g class="lower" transform="translate({sx} {sy})"><g clip-path="url(#{uid}-dn)">{glyph_paths()}</g></g>
  </g>
  <g id="{uid}-drop"><path transform="translate({dx + sx} {sy})" d="{dd}"/></g>
</defs>'''

    body = []
    if v["extrude"]:
        for i in range(16, 0, -1):
            t = f'translate({i*.5:.2f} {i*.68:.2f})'
            body.append(f'<use href="#{uid}-split" transform="{t}" fill="{v["extrude"]}"/><use href="#{uid}-drop" transform="{t}" fill="{v["extrude"]}"/>')
    if v["glow"]:
        fault_mid = [(x + sx / 2, y + sy / 2) for x, y in FAULT[1:-1]]
        body.append(f'<polyline class="fissure" points="{pts(fault_mid)}" fill="none" stroke="{v["glow"]}" stroke-width="9" filter="url(#{uid}-glow)"/>')
        body.append(f'<polyline class="fissure-core" points="{pts(fault_mid)}" fill="none" stroke="{v["glow_core"]}" stroke-width="1.6"/>')
    if v["outline"]:
        body.append(f'<use href="#{uid}-split" fill="none" stroke="{v["outline"]}" stroke-width="5" stroke-linejoin="round"/>')
        body.append(f'<use href="#{uid}-drop" fill="none" stroke="{v["outline"]}" stroke-width="5"/>')
    body.append(f'<use class="face" href="#{uid}-split" fill="{v["face"] or f"url(#{uid}-face)"}"/>')
    body.append(f'<use class="drop" href="#{uid}-drop" fill="{v["drop"]}"/>')
    # the hourglass glass breaks along the same fault as the letters
    glass = hourglass_glass(v["hg_edge"], v["sand"], v["glass"])
    body.append(f'<g clip-path="url(#{uid}-up)">{glass}</g>'
                f'<g class="lower" transform="translate({sx} {sy})"><g clip-path="url(#{uid}-dn)">{glass}</g></g>')
    if v["water"]:
        body.append(f'<g class="lower" transform="translate({sx} {sy})"><g clip-path="url(#{uid}-dn)"><g clip-path="url(#{uid}-letters)">'
                    f'<rect class="flood" x="-10" y="108" width="640" height="40" fill="url(#{uid}-water)"/></g></g></g>')
    if v["cracks"]:
        cracks = ["M96 61L103 47L99 38", "M150 78L143 90", "M262 86L256 70L262 60", "M318 80L326 66",
                  "M58 68L52 54", "M206 71L214 58", "M470 99L478 84L474 76", "M530 93L536 80"]
        body.append(f'<g clip-path="url(#{uid}-up)"><g clip-path="url(#{uid}-letters)" fill="none" stroke="{v["cracks"]}" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">'
                    + "".join(f'<path d="{c}"/>' for c in cracks) + "</g></g>")
    panic = defs + "".join(body)

    s = v["line"]
    seis = [(110, 0), (430, 0), (442, -8), (452, 12), (464, -28), (478, 26), (490, -40), (502, 34),
            (516, -22), (528, 14), (540, -8), (552, 4), (562, 0), (890, 0)]
    seis_d = "M" + " L".join(f"{x:g} {y:g}" for x, y in seis)
    # "TO" on a torn strip of warning tape, slightly askew
    tape = "M-56 -26H56L63 -17L57 -8L64 0L57 9L63 17L56 26H-56L-62 17L-57 9L-64 0L-57 -8L-63 -17Z"
    tape_svg = ""
    if v["badge"]:
        tape_svg = (f'<path d="{tape}" transform="translate(4 5)" fill="{v["badge_edge"]}"/>'
                    f'<path d="{tape}" fill="{v["badge"]}"/>')
    to = '<path d="M-36 -16H-6M-21 -16V18"/><ellipse cx="18" cy="-1" rx="12" ry="15"/>'
    to_shadow = f'<g transform="translate(2.5 3)" fill="none" stroke="{v["badge_edge"]}" stroke-width="8">{to}</g>' if v["badge"] else ""
    badge = (f'<g class="badge" transform="translate(500 178) rotate(-4)">{tape_svg}{to_shadow}'
             f'<g fill="none" stroke="{v["badge_text"]}" stroke-width="8" stroke-linejoin="miter">{to}</g></g>')
    wx, wy = WORD_POS
    return f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 40 1000 500" width="1000" height="500" role="img" aria-label="Too Late to Panic!">
<title>Too Late to Panic!</title>
<g class="row1" transform="translate(244 58)">{top_row(v["top"], v["top_shadow"], v["hands"])}</g>
{badge}
<g class="panic" transform="translate({wx} {wy}) scale({WORD_SCALE})">{panic}</g>
<path class="seismo" d="{seis_d}" transform="translate(0 492)" fill="none" stroke="{v["seismo"]}" stroke-width="3.4" stroke-linejoin="round" stroke-linecap="round"/>
</svg>
'''


# -------------------------------------------------------------- emblem / icon
# One metaphor (hourglass) + one modifier (fault split): filled shapes only,
# so it survives 16 px, single colour and greyscale.
MARK_FAULT = [(-10, 112), (62, 122), (98, 114), (140, 136), (178, 128), (266, 150)]
MARK_SHIFT = (7, 8)


MARK_BODY = ("M58 44A6 6 0 0 1 64 38H192A6 6 0 0 1 198 44V59H186C186 95 148 113 138 128C148 143 186 161 186 197H198"
             "V212A6 6 0 0 1 192 218H64A6 6 0 0 1 58 212V197H70C70 161 108 143 118 128C108 113 70 95 70 59H58Z")
MARK_WINDOW = "M90 69H166C166 96 140 113 132 123V133C140 143 166 160 166 187H90C90 160 116 143 124 133V123C116 113 90 96 90 69Z"


def mark_hourglass(v, uid):
    """The same hourglass silhouette as the I in the wordmark, scaled to the tile."""
    sand = (f'<path d="M104 100H152C146 109 136 116 128 120C120 116 110 109 104 100Z" fill="{v["sand"]}"/>'
            f'<rect x="125.5" y="120" width="5" height="52" fill="{v["sand"]}"/>'
            f'<path d="M94 187C100 169 118 161 128 160C138 161 156 169 162 187Z" fill="{v["sand"]}"/>')
    if v.get("mono"):  # one colour: the glass is a cut-out so the silhouette still reads
        return f'<path d="{MARK_BODY} {MARK_WINDOW}" fill-rule="evenodd" fill="{v["frame"]}"/>' + sand
    return (f'<path d="{MARK_BODY}" transform="translate(6 7)" fill="{v["shadow"]}"/>'
            f'<path d="{MARK_BODY}" fill="none" stroke="{v["frame"]}" stroke-width="8" stroke-linejoin="round"/>'
            f'<path d="{MARK_BODY}" fill="url(#{uid}-face)"/>'
            f'<path d="{MARK_WINDOW}" fill="{v["glass"]}" stroke="{v["frame"]}" stroke-width="3" stroke-linejoin="round"/>'
            + sand +
            f'<path d="M98 77C98 91 108 101 118 110" fill="none" stroke="{v["frame"]}" stroke-width="3" stroke-linecap="round" opacity=".45"/>')


def build_mark(v):
    uid = v["id"]
    up = [(-20, -20), (276, -20), (276, 150)] + list(reversed(MARK_FAULT)) + [(-20, 112)]
    dn = MARK_FAULT + [(276, 150), (276, 276), (-20, 276), (-20, 112)]
    sx, sy = v.get("shift", MARK_SHIFT)
    hg = mark_hourglass(v, uid)
    tile = ""
    if v["tile"]:
        tile = (f'<rect x="8" y="8" width="240" height="240" rx="56" fill="url(#{uid}-tile)"/>'
                f'<rect x="8" y="8" width="240" height="240" rx="56" fill="url(#{uid}-heat)"/>')
    crack = ""
    if v["glow"]:
        mid = [(x + sx / 2, y + sy / 2) for x, y in MARK_FAULT]
        crack = (f'<g clip-path="url(#{uid}-tileclip)"><polyline points="{pts(mid)}" fill="none" stroke="{v["glow"]}" stroke-width="10" filter="url(#{uid}-blur)" opacity=".9"/>'
                 f'<polyline points="{pts(mid)}" fill="none" stroke="{v["crack"]}" stroke-width="5" stroke-linejoin="round"/>'
                 f'<polyline points="{pts(mid)}" fill="none" stroke="{v["glow_core"]}" stroke-width="1.6" stroke-linejoin="round"/></g>')
    return f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256" width="256" height="256" role="img" aria-label="Too Late to Panic! amblem">
<title>Too Late to Panic! amblem</title>
<defs>
  <linearGradient id="{uid}-tile" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="{v["tile"] or "none"}"/><stop offset="1" stop-color="{v["tile2"] or "none"}"/></linearGradient>
  <radialGradient id="{uid}-heat" cx=".5" cy="1" r=".75"><stop offset="0" stop-color="{RUST}" stop-opacity="{v["heat"]}"/><stop offset="1" stop-color="{RUST}" stop-opacity="0"/></radialGradient>
  <linearGradient id="{uid}-face" x1="0" y1="38" x2="0" y2="218" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#F7C35E"/><stop offset=".55" stop-color="{AMBER}"/><stop offset="1" stop-color="{RUST}"/></linearGradient>
  <filter id="{uid}-blur" x="-10%" y="-50%" width="120%" height="200%"><feGaussianBlur stdDeviation="4"/></filter>
  <clipPath id="{uid}-tileclip"><rect x="8" y="8" width="240" height="240" rx="56"/></clipPath>
  <clipPath id="{uid}-up"><polygon points="{pts(up)}"/></clipPath>
  <clipPath id="{uid}-dn"><polygon points="{pts(dn)}"/></clipPath>
</defs>
{tile}
{crack}
<g clip-path="url(#{uid}-up)">{hg}</g>
<g class="lower" transform="translate({sx} {sy})"><g clip-path="url(#{uid}-dn)">{hg}</g></g>
</svg>
'''


WORD_VARIANTS = {
    "logo": dict(id="d", face=None, face_top="#F7C35E", face_mid=AMBER, face_bot=RUST, outline=CREAM, extrude="#4A1D12",
                 glow=AMBER, glow_core="#FFE7B0", water="rgba(26,24,23,.32)", cracks="rgba(26,24,23,.55)",
                 hg_edge=CREAM, sand="#FFD27A", drop=ALARM, glass="#2E2725",
                 top=CREAM, top_shadow=RUST, hands=AMBER, line="rgba(246,241,232,.55)", accent=RUST,
                 badge=RUST, badge_edge="#4A1D12", badge_text=CREAM, seismo=RUST),
    "logo-light": dict(id="l", face=None, face_top="#F5B54A", face_mid="#E79A36", face_bot=RUST, outline=INK, extrude=INK,
                       glow=AMBER, glow_core="#FFF3D6", water="rgba(26,24,23,.28)", cracks="rgba(26,24,23,.6)",
                       hg_edge=INK, sand="#E79A36", drop=ALARM, glass="#FBF6EE",
                       top=INK, top_shadow=RUST, hands=RUST, line="rgba(41,38,37,.55)", accent=RUST,
                       badge=RUST, badge_edge=INK, badge_text=CREAM, seismo=RUST),
    "logo-mono": dict(id="m", face=CREAM, face_top=CREAM, face_mid=CREAM, face_bot=CREAM, outline=None, extrude=None,
                      glow=None, glow_core=None, water=None, cracks=None,
                      hg_edge=CREAM, sand=CREAM, drop=CREAM, glass="none",
                      top=CREAM, top_shadow=None, hands=CREAM, line=CREAM, accent=CREAM,
                      badge=None, badge_edge=None, badge_text=CREAM, seismo=CREAM),
}
MARK_VARIANTS = {
    "mark": dict(id="md", tile="#2E2A27", tile2=DEEP, heat=".55", frame=CREAM, glass="#141211", sand="#FFD27A", shadow="rgba(0,0,0,.5)",
                 glow=AMBER, crack=DEEP, glow_core="#FFE7B0"),
    "mark-light": dict(id="ml", tile=CREAM, tile2="#E9E1D4", heat=".18", frame=INK, glass="#FBF6EE", sand="#E08A2E", shadow="rgba(41,38,37,.3)",
                       glow=AMBER, crack=INK, glow_core="#FFF3D6"),
    # 16-32 px: same silhouette without the fault, per small-size icon guidance
    "mark-small": dict(id="mx", tile="#2E2A27", tile2=DEEP, heat=".55", frame=CREAM, glass="#141211", sand="#FFD27A", shadow="rgba(0,0,0,.5)",
                       glow=None, crack=None, glow_core=None, shift=(0, 0)),
    "mark-mono": dict(id="mm", tile=None, tile2=None, heat="0", frame=CREAM, glass="none", sand=CREAM, mono=True,
                      glow=None, crack=None, glow_core=None),
}

if __name__ == "__main__":
    for name, v in WORD_VARIANTS.items():
        with open(os.path.join(HERE, name + ".svg"), "w", encoding="utf-8") as f:
            f.write(build_wordmark(v))
    for name, v in MARK_VARIANTS.items():
        with open(os.path.join(HERE, name + ".svg"), "w", encoding="utf-8") as f:
            f.write(build_mark(v))
    print("written:", ", ".join(list(WORD_VARIANTS) + list(MARK_VARIANTS)))

"""Render the SVG logo variants to transparent PNGs with headless Chrome.

Run:  python render_png.py   (after build_logo.py)
"""
import os
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "png")
CHROME = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

# name -> (svg file, css width, css height, device scale)
JOBS = {
    "logo-2000": ("logo.svg", 1000, 500, 2),
    "logo-1000": ("logo.svg", 1000, 500, 1),
    "logo-light-2000": ("logo-light.svg", 1000, 500, 2),
    "logo-mono-2000": ("logo-mono.svg", 1000, 500, 2),
    "mark-512": ("mark.svg", 256, 256, 2),
    "mark-256": ("mark.svg", 256, 256, 1),
    "mark-light-512": ("mark-light.svg", 256, 256, 2),
    "mark-mono-512": ("mark-mono.svg", 256, 256, 2),
    "mark-32": ("mark-small.svg", 32, 32, 1),
    "mark-16": ("mark-small.svg", 16, 16, 1),
}


def render(name, svg, w, h, scale, background=None):
    os.makedirs(OUT, exist_ok=True)
    page = os.path.join(OUT, f"_{name}.html")
    bg = background or "transparent"
    with open(page, "w", encoding="utf-8") as f:
        f.write(f'<!doctype html><html><body style="margin:0;background:{bg}">'
                f'<img src="../{svg}" width="{w}" height="{h}" style="display:block"></body></html>')
    target = os.path.join(OUT, name + ".png")
    subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--hide-scrollbars",
                    f"--force-device-scale-factor={scale}", f"--window-size={w},{h}",
                    "--default-background-color=00000000", f"--screenshot={target}",
                    "file:///" + page.replace("\\", "/")], check=True, capture_output=True)
    os.remove(page)
    return target


if __name__ == "__main__":
    for name, job in JOBS.items():
        print(render(name, *job))

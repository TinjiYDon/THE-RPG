#!/usr/bin/env python3
"""
Copy Unity events JSON from Assets/Content/events into mobile/assets/events.
Flattens structure by copying all *.json under events/ to single target folder.
"""
import os
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / 'Assets' / 'Content' / 'events'
DST = ROOT / 'mobile' / 'assets' / 'events'

def main():
    if not SRC.exists():
        print(f'Source not found: {SRC}')
        return 1
    DST.mkdir(parents=True, exist_ok=True)
    # Clear target folder
    for p in DST.glob('*.json'):
        try:
            p.unlink()
        except Exception:
            pass
    count = 0
    for p in SRC.rglob('*.json'):
        try:
            shutil.copy2(p, DST / p.name)
            count += 1
        except Exception as e:
            print(f'Copy failed {p}: {e}')
    print(f'Synced {count} files to {DST}')
    return 0

if __name__ == '__main__':
    raise SystemExit(main())



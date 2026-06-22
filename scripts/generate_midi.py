#!/usr/bin/env python3
"""Generate a deterministic MIDI track for a song using musiclang_predict."""

from __future__ import annotations

import argparse
import random
import sys
from pathlib import Path

GENRE_ALIASES: dict[str, str] = {
    "elektronisch": "electronic",
    "klassik": "classical",
}

GENRE_PROGRESSIONS: dict[str, list[str]] = {
    "rock": ["Am FM CM GM", "Em CM GM DM", "AM DM EM AM", "Dm BbM FM CM"],
    "pop": ["CM GM Am FM", "GM DM Em CM", "FM CM Dm BbM", "AM EM F#m DM"],
    "jazz": ["Dm7 G7 CM", "Am7 D7 GM7 CM", "Em7 A7 DM", "Cm7 F7 BbM"],
    "hip-hop": ["Am FM CM GM", "Dm BbM FM CM", "Em CM GM DM"],
    "electronic": ["Am FM CM GM", "Dm Am BbM FM", "Em Am DM GM"],
    "classical": ["Am Dm EM Am", "Cm Fm GM Cm", "Dm GM CM Dm", "Em Am BM Em"],
    "r&b": ["Am7 Dm7 GM CM", "FM Am BbM CM", "Dm7 G7 CM"],
    "metal": ["Em CM DM BM", "Am FM GM EM", "Dm BbM CM AM"],
    "country": ["GM CM DM GM", "AM DM EM AM", "CM FM GM CM"],
    "blues": ["A7 D7 E7 A7", "E7 A7 B7 E7", "G7 C7 D7 G7"],
    "reggae": ["Am Dm GM Am", "Cm FM GM Cm", "Em Am DM GM"],
    "techno": ["Am FM CM GM", "Dm Am BbM FM", "Em CM GM DM"],
}

DEFAULT_PROGRESSIONS = ["CM GM Am FM", "Am FM CM GM", "Dm BbM FM CM", "Em CM GM DM"]

TEMPO_BY_GENRE: dict[str, int] = {
    "rock": 120,
    "pop": 110,
    "jazz": 100,
    "hip-hop": 90,
    "electronic": 128,
    "classical": 96,
    "r&b": 85,
    "metal": 140,
    "country": 105,
    "blues": 80,
    "reggae": 92,
    "techno": 130,
}


def normalize_genre(genre: str) -> str:
    key = genre.strip().lower()
    return GENRE_ALIASES.get(key, key)


def pick_progression(genre: str, rng_seed: int) -> str:
    rng = random.Random(rng_seed)
    key = normalize_genre(genre)
    progressions = GENRE_PROGRESSIONS.get(key, DEFAULT_PROGRESSIONS)
    return progressions[rng.randint(0, len(progressions) - 1)]


def pick_tempo(genre: str, rng_seed: int) -> int:
    rng = random.Random(rng_seed ^ 0x5DEECE66D)
    key = normalize_genre(genre)
    base = TEMPO_BY_GENRE.get(key, 110)
    return max(70, min(160, base + rng.randint(-8, 8)))


def generate_midi(output_path: Path, rng_seed: int, genre: str) -> None:
    from musiclang_predict import MusicLangPredictor

    progression = pick_progression(genre, rng_seed)
    tempo = pick_tempo(genre, rng_seed)
    temperature = 0.75 + (abs(rng_seed) % 16) / 100.0

    predictor = MusicLangPredictor("musiclang/musiclang-v2")
    score = predictor.predict_chords(
        progression,
        time_signature=(4, 4),
        nb_tokens=768,
        temperature=temperature,
        topp=1.0,
        rng_seed=rng_seed if rng_seed != 0 else 42,
    )
    score.to_midi(str(output_path), tempo=tempo, time_signature=(4, 4))


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate a MIDI file with musiclang_predict")
    parser.add_argument("--rng-seed", type=int, required=True, help="Deterministic RNG seed")
    parser.add_argument("--genre", type=str, required=True, help="Song genre")
    parser.add_argument("--output", type=Path, required=True, help="Output .mid path")
    args = parser.parse_args()

    args.output.parent.mkdir(parents=True, exist_ok=True)
    generate_midi(args.output, args.rng_seed, args.genre)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(1) from exc

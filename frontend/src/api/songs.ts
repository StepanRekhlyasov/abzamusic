export const INFINITE_SONGS_TOTAL = 2_147_483_647;

export function isInfiniteSongsTotal(totalCount: number): boolean {
  return totalCount >= INFINITE_SONGS_TOTAL;
}

export type Song = {
  id: number;
  index: number;
  title: string;
  artist: string;
  album: string;
  genre: string;
  likes: number;
  coverUrl: string;
  previewUrl: string;
  midiUrl: string;
};

export type SongsPageResponse = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  items: Song[];
};

export type GenerateSongsParams = {
  page: number;
  seed: string;
  likes: number;
  size: number;
  lang: string;
};

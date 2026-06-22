export type Song = {
  id: number;
  index: number;
  title: string;
  artist: string;
  album: string;
  genre: string;
  likes: number;
  coverUrl: string;
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
};

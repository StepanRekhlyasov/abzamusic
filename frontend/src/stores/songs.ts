import { defineStore } from 'pinia';
import { computed, ref, watch } from 'vue';

import type { GenerateSongsParams, Song, SongsPageResponse } from '@/api/songs';

const BASE62_ALPHABET = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
const UINT64_MAX = 2n ** 64n - 1n;

function parseBase62(value: string): bigint | null {
  let result = 0n;

  for (const character of value) {
    const digit = BASE62_ALPHABET.indexOf(character);
    if (digit < 0) return null;

    result = result * 62n + BigInt(digit);
    if (result > UINT64_MAX) return null;
  }

  return result;
}

function encodeBase62(value: bigint): string {
  if (value === 0n) return '0';

  let result = '';
  let current = value;

  while (current > 0n) {
    const remainder = Number(current % 62n);
    result = BASE62_ALPHABET[remainder] + result;
    current /= 62n;
  }

  return result;
}

export function isValidSeed(value: string): boolean {
  if (!value || !/^[0-9a-zA-Z]+$/.test(value)) return false;
  return parseBase62(value) !== null;
}

export function randomSeed(): string {
  const bytes = new Uint8Array(8);
  crypto.getRandomValues(bytes);

  let value = 0n;
  for (const byte of bytes) {
    value = (value << 8n) | BigInt(byte);
  }

  return encodeBase62(value);
}

type TablePagination = {
  page: number;
  rowsPerPage: number;
  rowsNumber?: number;
  sortBy: string;
  descending: boolean;
};

async function fetchSongs(params: GenerateSongsParams): Promise<SongsPageResponse> {
  const query = new URLSearchParams({
    page: String(params.page),
    seed: params.seed,
    likes: params.likes.toFixed(1),
    size: String(params.size),
  });

  const response = await fetch(`/api/generate/songs?${query.toString()}`);

  if (!response.ok) {
    throw new Error(`Failed to load songs: ${response.status}`);
  }

  return response.json() as Promise<SongsPageResponse>;
}

export const useSongsStore = defineStore('songs', () => {
  const enableVirtualScroll = ref(false);
  const pageSize = ref(10);
  const seed = ref('5Ezg7yb1S');
  const likes = ref(5.0);
  const loading = ref(false);
  const totalPages = ref(1);
  const loadedPages = ref(0);
  const rowsByPage = ref<Song[][]>([]);
  let requestId = 0;

  const pagination = ref<TablePagination>({
    page: 1,
    rowsPerPage: pageSize.value,
    rowsNumber: 0,
    sortBy: 'index',
    descending: false,
  });

  const rows = computed(() => rowsByPage.value.flat());

  async function loadInfinitePage(page: number, append = false) {
    if (!isValidSeed(seed.value)) return;

    const currentRequestId = ++requestId;
    loading.value = true;

    try {
      const response = await fetchSongs({
        page,
        seed: seed.value,
        likes: likes.value,
        size: pageSize.value,
      });

      if (currentRequestId !== requestId) return;

      totalPages.value = response.totalPages;
      loadedPages.value = page;
      rowsByPage.value = append
        ? [...rowsByPage.value, response.items]
        : [response.items];
    } finally {
      if (currentRequestId === requestId) loading.value = false;
    }
  }

  async function onRequest(requestedPagination: TablePagination) {
    if (enableVirtualScroll.value) return;
    if (!isValidSeed(seed.value)) return;

    const currentRequestId = ++requestId;
    loading.value = true;

    try {
      const response = await fetchSongs({
        page: requestedPagination.page,
        seed: seed.value,
        likes: likes.value,
        size: pageSize.value,
      });

      if (currentRequestId !== requestId) return;

      totalPages.value = response.totalPages;
      loadedPages.value = requestedPagination.page;
      rowsByPage.value = [response.items];
      pagination.value = {
        ...requestedPagination,
        rowsNumber: response.totalCount,
        rowsPerPage: pageSize.value,
      };
    } finally {
      if (currentRequestId === requestId) loading.value = false;
    }
  }

  async function loadNextInfinitePage() {
    if (!enableVirtualScroll.value || loading.value) return false;
    if (loadedPages.value >= totalPages.value || rows.value.length === 0) return false;
    await loadInfinitePage(loadedPages.value + 1, true);
    return true;
  }

  const canLoadMoreInfinite = computed(
    () =>
      enableVirtualScroll.value &&
      !loading.value &&
      loadedPages.value > 0 &&
      loadedPages.value < totalPages.value,
  );

  async function reloadCurrentMode() {
    loadedPages.value = 0;
    rowsByPage.value = [];
    requestId++;

    if (enableVirtualScroll.value) {
      pagination.value = { page: 1, rowsPerPage: 0, sortBy: 'index', descending: false };
      delete pagination.value.rowsNumber;
      await loadInfinitePage(1, false);
      return;
    }

    pagination.value = { page: 1, rowsPerPage: pageSize.value, rowsNumber: 0, sortBy: 'index', descending: false };
    await onRequest(pagination.value);
  }

  watch([enableVirtualScroll, seed, likes], () => {
    void reloadCurrentMode();
  }, { immediate: true });

  return {
    enableVirtualScroll,
    pageSize,
    seed,
    likes,
    loading,
    totalPages,
    loadedPages,
    rowsByPage,
    pagination,
    rows,
    canLoadMoreInfinite,
    onRequest,
    loadNextInfinitePage,
    reloadCurrentMode,
  };
});

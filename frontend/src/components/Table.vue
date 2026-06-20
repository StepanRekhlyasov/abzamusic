<template>
  <q-table
    class="sticky-header-table"
    flat
    bordered
    row-key="id"
    :rows="rows"
    :columns="columns"
    :loading="loading"
    :virtual-scroll="enableVirtualScroll"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    v-model:pagination="pagination"
    :rows-per-page-options="enableVirtualScroll ? [0] : [pageSize]"
    @virtual-scroll="onScroll"
  >
    <template #top>
      <Toolbar v-model="enableVirtualScroll" />
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch, type Component } from 'vue';
import type { QTableProps } from 'quasar';

import Toolbar from '@/components/Toolbar.vue';

const enableVirtualScroll = defineModel<boolean>({ default: false });

const pageSize = 10;

const genres = [
  'Rock',
  'Pop',
  'Jazz',
  'Hip-Hop',
  'Electronic',
  'Classical',
  'R&B',
  'Metal',
  'Country',
  'Blues',
  'Indie',
  'Reggae',
]

const artists = [
  'The Echoes',
  'Luna Ray',
  'Midnight Bloom',
  'Velvet Horizon',
  'Neon Parade',
  'Silver Atlas',
  'Crimson Tide',
  'Aurora Fields',
  'Static Hearts',
  'Ocean Drive',
  'Paper Satellites',
  'Golden Frames',
  'Wild Syntax',
  'Northern Lights',
  'Blue Comet',
]

const albumPrefixes = [
  'Echoes of',
  'Letters from',
  'Stories in',
  'Shadows on',
  'Colors of',
  'Voices in',
  'Dreams of',
  'Maps to',
  'Fragments of',
  'Light in',
]

const albumPlaces = [
  'Tomorrow',
  'Midnight',
  'Summer',
  'Winter',
  'Neon City',
  'The Valley',
  'Lost Time',
  'Silent Rooms',
  'Broken Radio',
  'Open Roads',
]

const songAdjectives = [
  'Silent',
  'Golden',
  'Broken',
  'Electric',
  'Fading',
  'Velvet',
  'Neon',
  'Wandering',
  'Crystal',
  'Burning',
  'Frozen',
  'Hidden',
]

const songNouns = [
  'Rain',
  'Highway',
  'Heartbeat',
  'Skies',
  'Echo',
  'Horizon',
  'Mirrors',
  'Firelight',
  'Satellite',
  'Waves',
  'Parade',
  'Shadows',
]

const allRows = Array.from({ length: 101 }, (_, index) => ({
  id: index + 1,
  index: index + 1,
  song: `${songAdjectives[index % songAdjectives.length]!} ${songNouns[(index + 3) % songNouns.length]!}`,
  artist: artists[index % artists.length]!,
  album: `${albumPrefixes[index % albumPrefixes.length]!} ${albumPlaces[(index + 5) % albumPlaces.length]!}`,
  genre: genres[index % genres.length]!,
}));

const lastPage = Math.ceil(allRows.length / pageSize);

const columns: QTableProps['columns'] = [
  { name: 'index', label: '#', field: 'index', align: 'left', sortable: true, style: 'width: 60px' },
  { name: 'song', label: 'Song', field: 'song', align: 'left', sortable: true },
  { name: 'artist', label: 'Artist', field: 'artist', align: 'left', sortable: true },
  { name: 'album', label: 'Album', field: 'album', align: 'left', sortable: true },
  { name: 'genre', label: 'Genre', field: 'genre', align: 'left', sortable: true },
];

const loading = ref(false);
const nextPage = ref(2);

const pagination = ref({
  page: 1,
  rowsPerPage: pageSize,
  sortBy: 'index',
  descending: false,
});

const rows = computed(() =>
  enableVirtualScroll.value ? allRows.slice(0, pageSize * (nextPage.value - 1)) : allRows,
);

function onScroll(details: {
  to: number;
  ref: Component & { refresh?: () => void };
}) {
  const { to, ref: compRef } = details;
  const lastIndex = rows.value.length - 1;

  if (loading.value || nextPage.value >= lastPage || to !== lastIndex) {
    return;
  }

  loading.value = true;

  setTimeout(() => {
    nextPage.value++;
    void nextTick(() => {
      compRef.refresh?.();
      loading.value = false;
    });
  }, 300);
}

watch(enableVirtualScroll, (vs) => {
  pagination.value.page = 1;
  pagination.value.rowsPerPage = vs ? 0 : pageSize;
  nextPage.value = 2;
});
</script>

<style lang="scss" scoped>
.sticky-header-table {
  height: 100%;
  min-height: 0;

  :deep(.q-table__top),
  :deep(.q-table__bottom),
  :deep(thead tr:first-child th) {
    background-color: white;
  }

  :deep(thead tr th) {
    position: sticky;
    z-index: 1;
  }

  :deep(thead tr:first-child th) {
    top: 0;
  }

  :deep(tbody) {
    scroll-margin-top: 48px;
  }
}
</style>

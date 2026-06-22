<template>
  <q-table
    ref="tableRef"
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
    :virtual-scroll-slice-size="30"
    v-model:pagination="pagination"
    :rows-per-page-options="enableVirtualScroll ? [0] : [pageSize]"
    :hide-bottom="enableVirtualScroll"
    @request="handleRequest"
  >
    <template #top>
      <TableToolbar />
    </template>

    <template #body="props">
      <q-tr
        :props="props"
        :ref="(el) => bindLastRowObserver((el as ComponentPublicInstance), props.pageIndex)"
      >
        <q-td v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.value }}
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, watch, type ComponentPublicInstance } from 'vue';
import { storeToRefs } from 'pinia';
import type { QTable, QTableProps } from 'quasar';

import TableToolbar from '@/components/TableToolbar.vue';
import { useSongsStore } from '@/stores/songs';

const songsStore = useSongsStore();
const {
  enableVirtualScroll,
  pageSize,
  loading,
  pagination,
  rows,
} = storeToRefs(songsStore);

const tableRef = ref<QTable | null>(null);

type TablePagination = {
  page: number;
  rowsPerPage: number;
  rowsNumber?: number;
  sortBy: string;
  descending: boolean;
};

const columns: QTableProps['columns'] = [
  { name: 'index', label: '#', field: 'index', align: 'left', sortable: true, style: 'width: 60px' },
  { name: 'title', label: 'Song', field: 'title', align: 'left', sortable: true },
  { name: 'artist', label: 'Artist', field: 'artist', align: 'left', sortable: true },
  { name: 'album', label: 'Album', field: 'album', align: 'left', sortable: true },
  { name: 'genre', label: 'Genre', field: 'genre', align: 'left', sortable: true },
  { name: 'likes', label: 'Likes', field: 'likes', align: 'left', sortable: true, style: 'width: 80px' },
];

let observer: IntersectionObserver | null = null;
let observedLastRowIndex = -1;

function disconnectObserver() {
  observer?.disconnect();
  observer = null;
  observedLastRowIndex = -1;
}

async function handleLastRowVisible() {

  const loaded = await songsStore.loadNextInfinitePage();
  if (!loaded) return;

  await nextTick();
  tableRef.value?.resetVirtualScroll();
}

function bindLastRowObserver(
  el: ComponentPublicInstance | null,
  pageIndex: number,
) {
  if (!el) return;
  if (!enableVirtualScroll.value || pageIndex !== rows.value.length - 1) return;
  if (observedLastRowIndex === pageIndex) return;
  disconnectObserver();
  observedLastRowIndex = pageIndex;
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        void handleLastRowVisible();
      }
    },
    {
      root: el.$el.parentElement,
      threshold: 0.1,
    },
  );
  observer.observe(el.$el);
}

function handleRequest(requestProps: { pagination: TablePagination }) {
  void songsStore.onRequest(requestProps.pagination);
}

watch([rows, enableVirtualScroll], () => {
  disconnectObserver();
});

onBeforeUnmount(() => {
  disconnectObserver();
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

  :deep(.q-table__middle) {
    flex: 1 1 auto;
    min-height: 0;
  }

  :deep(tbody) {
    scroll-margin-top: 48px;
  }
}
</style>

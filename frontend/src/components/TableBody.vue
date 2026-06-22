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
    v-model:expanded="expanded"
    :pagination-label="formatPaginationLabel"
    :rows-per-page-options="enableVirtualScroll ? [0] : [pageSize]"
    :hide-bottom="enableVirtualScroll"
    @request="handleRequest"
  >
    <template #top>
      <TableToolbar />
    </template>

    <template #header="props">
      <q-tr :props="props">
        <q-th auto-width />
        <q-th
          v-for="col in props.cols"
          :key="col.name"
          :props="props"
        >
          {{ col.label }}
        </q-th>
      </q-tr>
    </template>

    <template #body="props">
      <q-tr
        :key="`main-${props.row.id}`"
        :props="props"
        :ref="(el) => bindLastRowObserver((el as ComponentPublicInstance), props.pageIndex)"
      >
        <q-td auto-width>
          <q-btn
            flat
            round
            dense
            size="sm"
            :icon="props.expand ? 'keyboard_arrow_up' : 'keyboard_arrow_down'"
            :aria-label="props.expand ? t('collapse') : t('expand')"
            @click="props.expand = !props.expand"
          />
        </q-td>
        <q-td v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.value }}
        </q-td>
      </q-tr>
      <q-tr
        v-if="props.expand"
        :key="`expand-${props.row.id}`"
        :props="props"
        class="q-virtual-scroll--with-prev expanded-row"
      >
        <q-td :colspan="props.cols.length + 1">
          <div class="expanded-content row items-center q-gutter-md q-pa-sm">
            <q-img
              :src="props.row.coverUrl"
              :alt="props.row.album"
              width="160px"
              height="160px"
              class="album-cover"
            />
            <div class="column">
              <span class="text-subtitle2 text-grey-7">{{ t('album') }}</span>
              <span class="text-body1">{{ props.row.album }}</span>
            </div>
          </div>
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, watch, computed, type ComponentPublicInstance } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import type { QTable, QTableProps } from 'quasar';

import TableToolbar from '@/components/TableToolbar.vue';
import { isInfiniteSongsTotal } from '@/api/songs';
import { useSongsStore } from '@/stores/songs';

const { t } = useI18n();
const songsStore = useSongsStore();
const {
  enableVirtualScroll,
  pageSize,
  loading,
  pagination,
  rows,
  hasInfiniteSongs,
} = storeToRefs(songsStore);

const tableRef = ref<QTable | null>(null);
const expanded = ref<(string | number)[]>([]);

type TablePagination = {
  page: number;
  rowsPerPage: number;
  rowsNumber?: number;
  sortBy: string;
  descending: boolean;
};

const columns = computed<QTableProps['columns']>(() => [
  { name: 'index', label: '#', field: 'index', align: 'left', sortable: false, style: 'width: 60px' },
  { name: 'title', label: t('columns.song'), field: 'title', align: 'left', sortable: false },
  { name: 'artist', label: t('columns.artist'), field: 'artist', align: 'left', sortable: false },
  { name: 'album', label: t('columns.album'), field: 'album', align: 'left', sortable: false },
  { name: 'genre', label: t('columns.genre'), field: 'genre', align: 'left', sortable: false },
  { name: 'likes', label: t('columns.likes'), field: 'likes', align: 'left', sortable: false, style: 'width: 80px' },
]);

function formatPaginationLabel(start: number, end: number, total: number) {
  if (hasInfiniteSongs.value || isInfiniteSongsTotal(total)) {
    return t('paginationRange', { start, end });
  }

  return t('paginationRangeOf', { start, end, total });
}

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
  expanded.value = [];
});

watch(expanded, async () => {
  if (!enableVirtualScroll.value) return;
  await nextTick();
  tableRef.value?.resetVirtualScroll();
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

.expanded-row {
  background-color: #fafafa;
}

.expanded-content {
  min-height: 160px;
}

.album-cover {
  border-radius: 4px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.2);
}
</style>

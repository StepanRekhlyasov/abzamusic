<template>
  <q-table
    flat
    bordered
    title="Список"
    row-key="id"
    :rows="rows"
    :columns="columns"
    :loading="loading"
    :virtual-scroll="enableVirtualScroll"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    v-model:pagination="pagination"
    :rows-per-page-options="enableVirtualScroll ? [0] : [pageSize]"
    :style="enableVirtualScroll ? 'height: calc(100vh - 180px)' : undefined"
    @virtual-scroll="onScroll"
  />
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch, type Component } from 'vue';
import type { QTableProps } from 'quasar';

const props = defineProps<{
  enableVirtualScroll: boolean;
}>();

const pageSize = 10;
const roles = ['Admin', 'User', 'Editor', 'Viewer'] as const;

const allRows = Array.from({ length: 87 }, (_, index) => ({
  id: index + 1,
  name: `User ${index + 1}`,
  email: `user${index + 1}@example.com`,
  role: roles[index % roles.length]!,
  createdAt: new Date(Date.now() - index * 86_400_000).toISOString().slice(0, 10),
}));

const lastPage = Math.ceil(allRows.length / pageSize);

const columns: QTableProps['columns'] = [
  { name: 'id', label: 'ID', field: 'id', align: 'left', sortable: true },
  { name: 'name', label: 'Имя', field: 'name', align: 'left', sortable: true },
  { name: 'email', label: 'Email', field: 'email', align: 'left', sortable: true },
  { name: 'role', label: 'Роль', field: 'role', align: 'left', sortable: true },
  { name: 'createdAt', label: 'Дата', field: 'createdAt', align: 'left', sortable: true },
];

const loading = ref(false);
const nextPage = ref(2);

const pagination = ref({
  page: 1,
  rowsPerPage: pageSize,
  sortBy: 'id',
  descending: false,
});

const rows = computed(() =>
  props.enableVirtualScroll ? allRows.slice(0, pageSize * (nextPage.value - 1)) : allRows,
);

watch(
  () => props.enableVirtualScroll,
  (vs) => {
    pagination.value.page = 1;
    pagination.value.rowsPerPage = vs ? 0 : pageSize;
    nextPage.value = 2;
  },
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
</script>

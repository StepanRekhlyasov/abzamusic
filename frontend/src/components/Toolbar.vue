<template>
  <q-toolbar class="q-mb-md rounded-borders bg-grey-2 text-dark">
    <q-toolbar-title class="text-subtitle1">Пользователи</q-toolbar-title>

    <q-space />

    <span class="q-mr-sm text-caption text-grey-7">Режим отображения</span>
    <q-btn-toggle
      v-model="enableVirtualScroll"
      no-caps
      unelevated
      toggle-color="primary"
      color="white"
      text-color="primary"
      :options="[
        { label: 'Пагинация', value: false },
        { label: 'Бесконечный скролл', value: true },
      ]"
    />
  </q-toolbar>
</template>

<script setup lang="ts">
const enableVirtualScroll = defineModel<boolean>({ default: false });
</script>

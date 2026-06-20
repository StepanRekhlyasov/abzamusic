<template>
  <q-toolbar class="full-width q-px-none bg-transparent">
    <q-toolbar-title class="text-h6">Треки</q-toolbar-title>

    <q-space />
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

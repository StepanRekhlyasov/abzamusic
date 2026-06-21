<template>
  <q-page padding class="tracks-page column no-wrap">
    <table-body class="col" />
  </q-page>
</template>

<script setup lang="ts">
import TableBody from '@/components/TableBody.vue';
</script>

<style scoped lang="scss">
.tracks-page {
  height: calc(100vh - var(--app-header-height, 50px));
  max-height: calc(100vh - var(--app-header-height, 50px));
  overflow: hidden;
}
</style>

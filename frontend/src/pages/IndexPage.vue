<template>
  <q-page padding>
    <Toolbar v-model="enableVirtualScroll" />
    <Table :enable-virtual-scroll="enableVirtualScroll" />
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';

import Table from '@/components/Table.vue';
import Toolbar from '@/components/Toolbar.vue';

const enableVirtualScroll = ref(false);
</script>

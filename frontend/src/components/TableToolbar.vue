<template>
  <q-toolbar class="full-width q-px-none bg-transparent column items-stretch q-gutter-y-sm">
    <div class="row items-center full-width">
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
    </div>

    <div class="row items-center q-col-gutter-md full-width">
      <div class="col-12 col-sm-4">
        <q-input
          v-model="seed"
          label="Seed"
          outlined
          dense
          mask="########"
          unmasked-value
          hint="8 цифр"
          :rules="[(value) => /^\d{8}$/.test(value ?? '') || 'Нужно 8 цифр']"
        />
      </div>

      <div class="col-12 col-sm-4">
        <q-slider
          v-model="likes"
          :min="0"
          :max="10"
          :step="0.1"
          label
          label-always
          color="primary"
        />
      </div>

      <div class="col-12 col-sm-4 text-body2">
        Лайки: {{ likes.toFixed(1) }}
      </div>
    </div>
  </q-toolbar>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { useSongsStore } from '@/stores/songs';
const { enableVirtualScroll, seed, likes } = storeToRefs(useSongsStore());
</script>

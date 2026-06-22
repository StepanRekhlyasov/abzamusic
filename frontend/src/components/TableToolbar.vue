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
          hint="64-bit, 0-9 a-z A-Z"
          :rules="[(value) => isValidSeed(value ?? '') || 'Нужно 64-bit значение (0-9, a-z, A-Z)']"
        >
          <template #append>
            <q-btn
              flat
              dense
              round
              icon="casino"
              color="primary"
              aria-label="Случайный seed"
              @click="seed = randomSeed()"
            >
              <q-tooltip>Случайный seed</q-tooltip>
            </q-btn>
          </template>
        </q-input>
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
import { isValidSeed, randomSeed, useSongsStore } from '@/stores/songs';
const { enableVirtualScroll, seed, likes } = storeToRefs(useSongsStore());
</script>

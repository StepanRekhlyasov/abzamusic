<template>
  <q-toolbar class="full-width q-px-none bg-transparent column items-stretch q-gutter-y-sm">
    <div class="row">
      <div class="row items-center full-width">
        <q-toolbar-title class="text-h6">{{ t('tracks') }}</q-toolbar-title>
        <q-space />
        <q-select
          v-model="language"
          :options="languageOptions"
          :label="t('language')"
          emit-value
          map-options
          dense
          outlined
          class="language-select q-mr-sm"
          style="min-width: 120px"
          @update:model-value="onLanguageChange"
        />
        <q-btn-group unelevated class="scroll-mode-toggle">
          <q-btn
            :color="enableVirtualScroll ? 'white' : 'primary'"
            :text-color="enableVirtualScroll ? 'primary' : 'white'"
            icon="pages"
            :aria-label="t('pagination')"
            @click="enableVirtualScroll = false"
          >
            <q-tooltip>{{ t('pagination') }}</q-tooltip>
          </q-btn>
          <q-btn
            :color="enableVirtualScroll ? 'primary' : 'white'"
            :text-color="enableVirtualScroll ? 'white' : 'primary'"
            icon="unfold_more"
            :aria-label="t('infiniteScroll')"
            @click="enableVirtualScroll = true"
          >
            <q-tooltip>{{ t('infiniteScroll') }}</q-tooltip>
          </q-btn>
        </q-btn-group>
      </div>
      <div class="row items-center q-col-gutter-md full-width">
        <div class="col-12 col-sm-4">
          <q-input
            v-model="seed"
            :label="t('seed')"
            outlined
            dense
            :hint="t('seedHint')"
            :rules="[(value) => isValidSeed(value ?? '') || t('seedInvalid')]"
          >
            <template #append>
              <q-btn
                flat
                dense
                round
                icon="casino"
                color="primary"
                :aria-label="t('randomSeed')"
                @click="seed = randomSeed()"
              >
                <q-tooltip>{{ t('randomSeed') }}</q-tooltip>
              </q-btn>
            </template>
          </q-input>
        </div>
        <div class="col-12 col-sm-4">
          <q-slider
            v-model="likesDraft"
            :min="0"
            :max="10"
            :step="0.1"
            label
            label-always
            color="primary"
            @change="commitLikes"
          />
        </div>
        <div class="col-12 col-sm-4 text-body2">
          {{ t('likes') }}: {{ likesDraft.toFixed(1) }}
        </div>
      </div>
    </div>
  </q-toolbar>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { isValidSeed, randomSeed, useSongsStore } from '@/stores/songs';
import {
  LOCALE_BY_LANGUAGE,
  type AppLanguage,
} from '@/i18n/languages';

const { t, locale } = useI18n();
const { enableVirtualScroll, seed, likes, language } = storeToRefs(useSongsStore());

const likesDraft = ref(likes.value);

const languageOptions = computed(() => [
  { label: 'English', value: 'en' },
  { label: 'Deutsch', value: 'de' },
]);

watch(likes, (value) => {
  likesDraft.value = value;
});

function commitLikes(value: number) {
  likes.value = value;
}

function onLanguageChange(value: AppLanguage) {
  locale.value = LOCALE_BY_LANGUAGE[value];
}
</script>

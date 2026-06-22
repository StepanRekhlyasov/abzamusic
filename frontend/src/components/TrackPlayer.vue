<template>
  <div class="track-player">
    <div v-if="loading" class="loading-row row items-center q-gutter-sm">
      <q-spinner color="primary" size="28px" />
      <span class="text-body2 text-grey-7">{{ t('loadingTrack') }}</span>
    </div>

    <span v-else-if="error" class="text-negative text-caption">{{ error }}</span>

    <div v-else class="column q-gutter-y-sm">
      <audio
        :src="audioObjectUrl ?? undefined"
        controls
        class="track-audio full-width"
        preload="auto"
      />
      <div class="row items-center">
        <q-btn
          flat
          dense
          icon="download"
          :label="t('downloadMidi')"
          :aria-label="t('downloadMidi')"
          @click="downloadMidi"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  previewUrl: string;
  midiUrl: string;
  title: string;
}>();

const { t } = useI18n();

const loading = ref(true);
const error = ref('');
const audioObjectUrl = ref<string | null>(null);

const downloadName = computed(
  () => `${props.title.replace(/[^\w\s-]/g, '').trim() || 'song'}.mid`,
);

function revokeAudioUrl() {
  if (audioObjectUrl.value) {
    URL.revokeObjectURL(audioObjectUrl.value);
    audioObjectUrl.value = null;
  }
}

function resetState() {
  revokeAudioUrl();
  loading.value = true;
  error.value = '';
}

async function loadTrack() {
  resetState();

  try {
    const response = await fetch(props.previewUrl);
    if (!response.ok) {
      throw new Error(response.status === 503 ? t('midiUnavailable') : t('previewLoadFailed'));
    }

    const blob = await response.blob();
    audioObjectUrl.value = URL.createObjectURL(blob);
  } catch (err) {
    error.value = err instanceof Error ? err.message : t('previewLoadFailed');
  } finally {
    loading.value = false;
  }
}

async function downloadMidi() {
  if (!audioObjectUrl.value) {
    return;
  }

  try {
    const response = await fetch(props.midiUrl);
    if (!response.ok) {
      throw new Error(response.status === 503 ? t('midiUnavailable') : t('midiLoadFailed'));
    }

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = downloadName.value;
    link.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    error.value = err instanceof Error ? err.message : t('midiLoadFailed');
  }
}

watch(
  () => [props.previewUrl, props.midiUrl] as const,
  () => {
    void loadTrack();
  },
);

onMounted(() => {
  void loadTrack();
});

onBeforeUnmount(() => {
  revokeAudioUrl();
});
</script>

<style scoped>
.track-player {
  width: 100%;
  max-width: 420px;
  min-height: 48px;
}

.loading-row {
  min-height: 48px;
}

.track-audio {
  height: 40px;
}
</style>

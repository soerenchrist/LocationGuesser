<script setup lang="ts">
import { computed, onMounted, reactive } from 'vue';
import type { LatLng } from 'leaflet';
import { getGameSet } from '../../api/api';
import { Image } from '../../api/types';
import GuessMap from './GuessMap.vue';
import router from '../../router';

type State = {
  isLoading: boolean,
  images: Image[],
  isError: boolean,
  isNotFound: boolean,
  currentIndex: number,
  guessPosition?: LatLng
}

const props = defineProps({
  slug: String
})

const state = reactive<State>({
  isLoading: true,
  isError: false,
  isNotFound: false,
  images: [],
  currentIndex: 0
})

const onSubmit = () => {
  if (state.currentIndex < state.images.length - 1) {
    state.currentIndex++;
  }
  state.guessPosition = undefined;
}

const fetchGameSet = async () => {
  const response = await getGameSet(props.slug!);

  if (response.state === 'success') {
    state.images = response.data;
  }
  else if (response.state === 'not-found') {
    state.isNotFound = true;
    router.push({ name: 'NotFound' });
  }
  else {
    state.isError = true;
  }

  state.isLoading = false;
}

const onMapClick = (latLng: LatLng) => {
  state.guessPosition = latLng;
}

const canSubmit = computed(() => {
  if (state.guessPosition === undefined) return false;

  return true;
})

onMounted(() => {
  fetchGameSet();
})
</script>

<template>
  <div v-if="state.isLoading">
    <h1 class="font-bold text-2xl">Loading...</h1>
  </div>
  <div v-else-if="state.isError">
    <h1 class="text-5xl">Something went wrong...</h1>
  </div>
  <div v-else>
    <div class="flex flex-row justify-between">
      <div v-if="state.images.length > 0">
        <img :src="state.images[state.currentIndex].url" class="w-full" />
      </div>
      <div>
        <guess-map :position="state.guessPosition" @click="onMapClick" />
        <div class="p-4">
          <button :disabled="!canSubmit" class="bg-teal-400 hover:bg-teal-500 disabled:bg-slate-400 py-2 rounded w-full" @click="onSubmit">Submit</button>
        </div>
      </div>
    </div>
  </div>
</template>
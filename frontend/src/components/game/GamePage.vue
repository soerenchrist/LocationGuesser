<script setup lang="ts">
import { computed, onMounted, reactive } from 'vue';
import type { LatLng } from 'leaflet';
import { getGameSet } from '../../api/api';
import { Image, ImageSet } from '../../api/types';
import GuessMap from './GuessMap.vue';
import router from '../../router';

type State = {
  isLoading: boolean,
  images: Image[],
  imageSet?: ImageSet,
  isError: boolean,
  isNotFound: boolean,
  currentIndex: number,
  guessPosition?: LatLng,
  selectedYear: number
}

const props = defineProps({
  slug: String
})

const state = reactive<State>({
  isLoading: true,
  isError: false,
  isNotFound: false,
  images: [],
  currentIndex: 0,
  selectedYear: 0
})

const onSubmit = () => {
  if (state.currentIndex < state.images.length - 1) {
    state.currentIndex++;
  }
  state.selectedYear = Math.round((state.imageSet!.upperYearRange + state.imageSet!.lowerYearRange) / 2);
  state.guessPosition = undefined;
}

const fetchGameSet = async () => {
  const response = await getGameSet(props.slug!);

  if (response.state === 'success') {
    state.images = response.data.images;
    state.imageSet = response.data.imageSet;
    state.selectedYear = Math.round((state.imageSet!.upperYearRange + state.imageSet!.lowerYearRange) / 2);
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
    <div class="grid grid-cols-2 gap-2">
      <div v-if="state.images.length > 0">
        <img :src="state.images[state.currentIndex].url" class="w-full" />
      </div>
      <div>
        <guess-map :position="state.guessPosition" @click="onMapClick" />
        <div class="col-span-2 h-10 p-4">
          <input type="range" :min="state.imageSet!.lowerYearRange" :max="state.imageSet!.upperYearRange"
            v-model="state.selectedYear" class="w-full slider" />
          <h4 class="font-bold">Year: {{ state.selectedYear }}</h4>
        </div>
        <div class="mt-8 p-4">
          <button :disabled="!canSubmit" class="bg-teal-400 hover:bg-teal-500 disabled:bg-slate-400 py-2 rounded w-full"
            @click="onSubmit">Submit</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.slider {
  -webkit-appearance: none;
  appearance: none;
  width: 100%;
  height: 25px;
  background: #d3d3d3;
  outline: none;
  opacity: 0.7;
  -webkit-transition: .2s;
  transition: opacity .2s;
}

.slider:hover {
  opacity: 1;
}

.slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 25px;
  height: 25px;
  background: rgb(45 212 191);
  cursor: pointer;
}

.slider::-moz-range-thumb {
  width: 25px;
  height: 25px;
  background: rgb(45 212 191);
  cursor: pointer;
}
</style>
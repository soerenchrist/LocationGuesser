<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { getGameSet } from '../../api/api';
import { Image } from '../../api/types';
import GuessMap from './GuessMap.vue';
import router from '../../router';

type State = {
  isLoading: boolean,
  images: Image[],
  isError: boolean,
  isNotFound: boolean,
  currentIndex: number
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

const next = () => {
  if (state.currentIndex < state.images.length - 1) {
    state.currentIndex++;
  }
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

onMounted(() => {
  fetchGameSet();
})
</script>

<template>
  <div v-if="state.isLoading">
    <h1>Loading...</h1>
  </div>
  <div>
    <div v-if="state.images.length > 0">
      <img :src="state.images[state.currentIndex].url" width="400" />
    </div>
    <guess-map />
  </div>
  <button @click="next">Next</button>
</template>
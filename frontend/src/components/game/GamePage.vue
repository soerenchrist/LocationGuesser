<script setup lang="ts">
  import { onMounted, reactive } from 'vue';
  import { getGameSet } from '../../api/api';
  import { Image } from '../../api/types';

  type State = {
    isLoading: boolean,
    images: Image[],
    currentIndex: number
  }

  const props = defineProps({
    slug: String
  })

  const state = reactive<State>({
    isLoading: true,
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

    state.isLoading = false;
    state.images = response;
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
  </div>
  <button @click="next">Next</button>

</template>
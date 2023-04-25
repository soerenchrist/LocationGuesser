<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { getImageSets } from '../../api/api';
import { ImageSet } from '../../api/types';
import Heading from '../common/Heading.vue';
import ImageSetCard from './ImageSetCard.vue';
type State = {
  isLoading: boolean;
  imageSets: ImageSet[];
  isError: boolean
}

const state = reactive<State>({ isLoading: true, isError: false, imageSets: [] });

const fetchImageSets = async () => {
  const response = await getImageSets();

  if (response.state === 'success') {
    state.imageSets = response.data;
  }
  else {
    state.isError = true;
  }
  state.isLoading = false;
}

onMounted(() => {
  fetchImageSets();
});
</script>

<template>
  <div class="flex flex-col items-center">
    <img width="200" class="mt-4" src="../../assets/logo.jpg" />
    <Heading title="Location Guesser" />
    <div class="w-full mt-8 p-4">
      <template v-if="state.isLoading">
        <div class="text-2xl font-bold mt-8">Loading...</div>
      </template>
      <template v-else>
        <div class="w-full grid grid-cols-3 gap-4">
          <template v-for="imageSet in state.imageSets">
            <ImageSetCard :imageSet="imageSet" />
          </template>
        </div>
      </template>
    </div>
  </div>
</template>
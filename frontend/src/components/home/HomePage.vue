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
    <div class="flex flex-wrap justify-center mt-12">
      <template v-if="state.isLoading">
        <div class="text-2xl font-bold mt-8">Loading...</div>
      </template>
      <template v-else>
        <template v-for="imageSet in state.imageSets">
          <div class="flex flex-col items-center m-4">
            <ImageSetCard :imageSet="imageSet" />
          </div>
        </template>
      </template>
    </div>
  </div>
</template>
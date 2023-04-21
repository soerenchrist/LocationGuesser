<script setup lang="ts">
import { onMounted, reactive } from 'vue';
import { getImageSets } from '../../api/api';
import { ImageSet } from '../../api/types';
import Heading from '../common/Heading.vue';
import ImageSetCard from './ImageSetCard.vue';
type State = {
  isLoading: boolean;
  imageSets: ImageSet[];
}

const state = reactive<State>({ isLoading: true, imageSets: [] });

const fetchImageSets = async () => {
  const response = await getImageSets();

  state.isLoading = false;
  state.imageSets = response;
}

onMounted(() => {
  fetchImageSets();
});
</script>

<template>
  <div class="flex flex-col items-center">
    <Heading title="Location Guesser" />
    <RouterLink to="/about" class="text-2xl font-bold mt-8">About</RouterLink>
    <div class="flex flex-wrap justify-center mt-8">
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
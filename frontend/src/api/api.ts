import { z } from "zod";
import { ImageSet, imageSet, ApiResult, game, Game } from "./types";

export async function getImageSets(): Promise<ApiResult<ImageSet[]>> {
  const response = await fetch(
    `${import.meta.env.VITE_API_ENDPOINT}/api/imagesets`
  );
  if (response.ok) {
    const schema = z.array(imageSet);
    const data = await response.json();
    const result = schema.parse(data);
    return {
      state: "success",
      code: 200,
      data: result,
    };
  }

  if (response.status === 404) {
    return {
      state: "not-found",
      code: 404,
    };
  }

  return {
    state: "server-error",
    code: 500,
  };
}

export async function getGameSet(slug: string): Promise<ApiResult<Game>> {
  const response = await fetch(
    `${import.meta.env.VITE_API_ENDPOINT}/api/games/${slug}`
  );
  if (response.ok) {
    const data = await response.json();
    const result = game.parse(data);
    return {
      state: "success",
      code: 200,
      data: result,
    };
  }

  if (response.status === 404) {
    return {
      state: "not-found",
      code: 404,
    };
  }

  return {
    state: "server-error",
    code: 500,
  };
}

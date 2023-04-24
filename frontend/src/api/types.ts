import { z } from "zod";

export const imageSet = z.object({
  title: z.string(),
  description: z.string(),
  slug: z.string(),
  imageCount: z.number(),
  tags: z.string(),
});

export const image = z.object({
  number: z.number(),
  setSlug: z.string(),
  description: z.string(),
  license: z.string(),
  year: z.number(),
  latitude: z.number(),
  longitude: z.number(),
  url: z.string(),
});

export type ImageSet = z.infer<typeof imageSet>;
export type Image = z.infer<typeof image>;

export type Success<T> = {
  state: "success";
  code: 200;
  data: T;
};

export type NotFound = {
  state: "not-found";
  code: 404;
};
export type ServerError = {
  state: "server-error";
  code: 500;
};

export type ApiResult<T> = Success<T> | NotFound | ServerError;

import { z } from "zod";

export const imageSet = z.object({
    title: z.string(),
    description: z.string(),
    slug: z.string(),
    imageCount: z.number(),
    tags: z.string()
});

export type ImageSet = z.infer<typeof imageSet>;
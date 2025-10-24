export interface NewsArticleSummaryDto {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  coverImageUrl?: string | null;
  authorName: string;
  publishedAtUtc: string;
}

export interface NewsArticleDto extends NewsArticleSummaryDto {
  content: string;
  updatedAtUtc: string;
}

export interface NewsArticleCreateDto {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl?: string;
}

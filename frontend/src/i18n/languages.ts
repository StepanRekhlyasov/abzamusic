export type AppLanguage = 'en' | 'de';

export const APP_LANGUAGES: AppLanguage[] = ['en', 'de'];

export const LOCALE_BY_LANGUAGE: Record<AppLanguage, 'en-US' | 'de-DE'> = {
  en: 'en-US',
  de: 'de-DE',
};

export const LANGUAGE_BY_LOCALE: Record<'en-US' | 'de-DE', AppLanguage> = {
  'en-US': 'en',
  'de-DE': 'de',
};

export function isAppLanguage(value: string): value is AppLanguage {
  return value === 'en' || value === 'de';
}

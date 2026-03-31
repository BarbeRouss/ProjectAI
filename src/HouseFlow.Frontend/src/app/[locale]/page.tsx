import Link from 'next/link';

export default async function RootPage({ params }: { params: Promise<{ locale: string }> }) {
  const { locale } = await params;

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 px-4">
      <h1 className="mb-8 text-4xl font-bold text-gray-900 dark:text-white">
        HouseFlow
      </h1>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src="https://media.giphy.com/media/U1XhGr8CWqvVC/giphy.gif"
        alt="Bryan"
        className="rounded-lg shadow-xl"
        width={480}
        height={270}
      />
      <p className="mt-6 text-lg text-gray-600 dark:text-gray-400">
        You&apos;re goddamn right.
      </p>
      <Link
        href={`/${locale}/login`}
        className="mt-8 rounded-lg bg-blue-600 px-6 py-3 text-white font-semibold hover:bg-blue-700 transition-colors"
      >
        Se connecter
      </Link>
    </div>
  );
}

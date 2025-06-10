/**
 * @type {import('eslint').FlatConfig}
 */
export default [
  {
    files: ['**/*.{js,jsx,ts,tsx}'],
    languageOptions: {
      parser: '@typescript-eslint/parser',
      parserOptions: { project: './tsconfig.json' },
    },
    plugins: {
      react: require('eslint-plugin-react'),
    },
    rules: {
      'react/jsx-uses-react': 'off',
      'react/react-in-jsx-scope': 'off',
    },
  },
]; 
const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
	mode: 'jit',
	purge: {
		content: [
			'./Alloy/Views/**/*.cshtml',
			'./Alloy/**/AlloyContentAreaRenderer.cs',
			'./stimulus_controllers/**/*.js',
		]
	},
	theme: {
		extend: {
			fontFamily: {
				sans: ['Inter var', ...defaultTheme.fontFamily.sans],
			},
			colors: {
				'alloy': {
					'50': '#f4f9fc', 
					'100': '#eaf2f8', 
					'200': '#cadfef', 
					'300': '#a9cce5', 
					'400': '#69a6d1', 
					'500': '#2980bd', 
					'600': '#2573aa', 
					'700': '#1f608e', 
					'800': '#194d71', 
					'900': '#143f5d'
				},
			},
		},
	},
	plugins: [
		require('@tailwindcss/forms'),
		require('@tailwindcss/typography'),
		require('@tailwindcss/aspect-ratio'),
	],
}

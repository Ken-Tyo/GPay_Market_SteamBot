const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
module.exports = {
  entry: {
    adminPage: './wwwroot/Source/containers/admin/index.js',
    homePage: './wwwroot/Source/containers/home/index.js',
  },
  output: {
    path: path.resolve(__dirname, 'wwwroot/dist'),
    filename: '[name].js',
  },
  module: {
    rules: [
      {
        test: /\.m?js$/,
        exclude: /(node_modules|bower_components)/,
        use: {
          loader: 'babel-loader',
          options: {
              presets: [["@babel/preset-react", { "runtime": "automatic" }], '@babel/preset-env'],
          },
        },
      },
      {
        test: /\.s[ac]ss$/i, // /\.scss$/,
        use: [
          MiniCssExtractPlugin.loader,
          //'css-loader',
          {
            loader: 'css-loader',
            options: {
              modules: {
                localIdentName: '[name]__[local]--[hash:base64:5]',
              },
            },
          },
          'sass-loader',
          // {
          //   loader: 'sass-loader',
          //   options: {
          //     implementation: 'node-sass', //sass
          //   },
          // },
        ],
      },
      {
        test: /\.(png|svg|jpg|jpeg|gif)$/i,
        type: 'asset/resource',
      },
    ],
  },
  plugins: [
    // extract css to external stylesheet file
    new MiniCssExtractPlugin({
      filename: 'build/[name].css',
    }),
  ],
  optimization: {
    minimize: false,
  },
};

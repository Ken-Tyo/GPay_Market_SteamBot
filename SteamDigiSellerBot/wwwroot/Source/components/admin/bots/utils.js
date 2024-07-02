export const stateColor = {
  1: "#3C965A",
  2: "#AD8D1D",
  3: "#A09F9B",
  4: "#CA2929",
};

export const currenciesSymbol = {
  RU: { symbol: "₽", flag: "" },
  UA: { symbol: "₴", flag: "" },
  KZ: { symbol: "₸", flag: "" },
  US: { symbol: "$", flag: "" },
  TR: { symbol: "TL", flag: "" },
  EU: { symbol: "€", flag: "" },
  CN: { symbol: "¥", flag: "" },
  VN: { symbol: "₫", flag: "" },
  UY: { symbol: "$U", flag: "" },
  KW: { symbol: "KWD", flag: "" },
  BR: { symbol: "R$", flag: "" },
  NZ: { symbol: "NZ$", flag: "" },
  IN: { symbol: "Rs", flag: "" },
  IL: { symbol: "ILS", flag: "" },
};

//иконки взяты тут https://icon666.com/ru/search?q=%D0%9D%D0%BE%D0%B2%D0%B0%D1%8F+%D0%97%D0%B5%D0%BB%D0%B0%D0%BD%D0%B4%D0%B8%D1%8F
export const getFlagByRegionCode = (code) => {
  if (!code) return "";
  var result;
  try {
    //result = require(`../../../icons/flags/CL.svg`);
    result = require(`../../../icons/flags/${code}.svg`);
  } catch {
    console.error(
      `Иконка для страны ${code} не найдена. Использован нейтральный флаг`
    );
    result = require(`../../../icons/flags/default.svg`);
  }

  return result;
};

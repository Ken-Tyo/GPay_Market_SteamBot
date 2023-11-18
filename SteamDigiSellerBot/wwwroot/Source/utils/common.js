export const mapToFormData = (val, keys) => {
  let data = new FormData();
  let obj = {};
  if (keys && keys.length) {
    keys.forEach((k) => {
      obj[k] = val[k];
    });
  } else {
    obj = val;
  }

  for (let prop in obj) {
    if (Array.isArray(val[prop])) {
      let elNum = 0;
      for (const a of val[prop]) {
        let key = prop + '[]';
        let vl = a;

        if (typeof a === 'object') {
          Object.entries(a).forEach((item, i) => {
            let key2 = `${prop}[${elNum}][${item[0]}]`;
            let vl2 = item[1];
            data.append(key2, vl2);
          });
        } else {
          data.append(key, vl);
        }

        elNum++;
      }
    } else {
      //console.log(prop, val[prop], val[prop] !== null);
      if (val[prop] !== null) data.append(prop, val[prop]);
    }
  }

  return data;
};

export const getUrlQueryParams = () => {
  const urlSearchParams = new URLSearchParams(window.location.search);
  const params = Object.fromEntries(urlSearchParams.entries());
  return params;
};

export const promisify = (f) => {
  return new Promise(async (res, rej) => {
    await f();
    res();
  });
};

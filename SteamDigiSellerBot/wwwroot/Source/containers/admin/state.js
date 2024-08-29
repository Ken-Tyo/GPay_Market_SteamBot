import { entity } from "simpler-state";
import { mapToFormData, getUrlQueryParams } from "../../utils/common";
import { itemsMode as iMode } from "../../containers/admin/common";
const signalR = require("@microsoft/signalr");

export const state = entity({
  user: { digisellerId: "", digisellerApiKey: "" },
  activeMenuLink: "",
  newUniqueCodes: [],
  items: [],
  itemsResponse: { loading: false, errors: [] },
  gameSessions: [],
  gameSessionsTotal: 0,
  gameSessionsFilter: {
    appId: "",
    gameName: "",
    orderId: null,
    profileStr: "",
    uniqueCodes: "",
    comment: "",
    steamCurrencyId: null,
    statusId: null,
    page: 1,
    size: 50,
  },
  productsFilter: {
    IsFilterOn: false,
  },
  gameSessionsStatuses: {},
  bots: [],
  proxies: [],
  selectedBot: {},
  selectedItem: {},
  selectedItems: [],
  exchageRates: [],
  currencies: [],
  steamRegions: [],
  digiPriceSetType: [
    { id: 1, name: "%" },
    { id: 2, name: "₽" },
  ],
  editBotResponse: { loading: false, errors: [] },
  editOrderResponse: { loading: false, errors: [] },
  saveBotRegionSetResponse: { loading: false, errors: [] },
  itemsMode: iMode[1],
  itemsLoading: true,
  bulkEditPercentModalIsOpen: false,
  changeItemBulkResponse: { loading: false, loadingItemInfo: false },
  digisellerEditModalIsOpen: false,
  changePasswordModalIsOpen: false,
  loadProxiesModalIsOpen: false,
  addGameSesCommentIsOpen: false,
  editBotModalIsOpen: false,
  exchangeRatesModalIsOpen: false,
  editItemModalIsOpen: false,
  editOrderModalIsOpen: false,
  filterOrdersModalIsOpen: false,
  filterProductsModalIsOpen: false,
  botRegionSetEditModalIsOpen: false,
  statusHistoryModalIsOpen: false,
  botDetailsModalIsOpen: false,
  orderCreationInfoIsOpen: false,
  editItemMainInfoModalIsOpen: false,
  editItemAdditionalInfoModalIsOpen: false,
  wsconn: null,
});

export const initAdmin = async () => {
  const { wsconn } = state.get();

  if (!wsconn) {
    let connection = new signalR.HubConnectionBuilder()
      //.configureLogging(signalR.LogLevel.Trace)
      .withUrl("/adminhub")
      .build();
    setStateProp("wsconn", connection);
    connection.on("Notify", async function (mes) {
      //let arg = data.arguments[0];
      console.log(mes);
      if (!mes) return;

      switch (mes.type) {
        case 1: {
          let gs = await apiFetchGameSession(mes.data.gsId);
          if (gs) {
            state.set((value) => {
              let gsList = value.gameSessions;
              let idx = gsList.map((gs) => gs.id).indexOf(gs.id);
              gsList[idx] = gs;

              return {
                ...value,
                gameSessions: gsList,
              };
            });
          }
        }
      }
    });

    await connection.start();
  }
};

export const initBotsPage = async () => {
  await apiFetchBots();
  await apiGetCurrencies();

  let params = getUrlQueryParams();
  console.log("params", params);
  let botId = Number(params.id || 0);
  if (botId) {
    const { bots } = state.get();
    let bot = bots.find((b) => b.id === botId);
    //console.log(botId, bots, bot);
    if (bot) {
      await setSelectedBot(bot);
      toggleBotDetailsModal(true);
    }
  }
};

export const setStateProp = (name, val) => {
  state.set((value) => {
    return {
      ...value,
      [name]: val,
    };
  });
};

export const setItems = (items) => {
  //items = Array(400).fill(items[0]);
  state.set((value) => {
    return {
      ...value,
      items: items,
    };
  });
};

export const setBots = (items) => {
  state.set((value) => {
    return {
      ...value,
      bots: items,
    };
  });
};

export const setProxies = (items) => {
  state.set((value) => {
    return {
      ...value,
      proxies: items,
    };
  });
};

export const setSelectedBot = async (bot) => {
  state.set((value) => {
    return {
      ...value,
      selectedBot: bot,
    };
  });
};

export const setSelectedItem = async (item) => {
  state.set((value) => {
    return {
      ...value,
      selectedItem: item,
    };
  });
};

export const setSelectedItems = (bot) => {
  state.set((value) => {
    return {
      ...value,
      selectedItems: bot,
    };
  });
};

export const setActiveMenuLink = (name) => {
  state.set((value) => {
    return {
      ...value,
      activeMenuLink: name,
    };
  });
};

const isNullOrEmpty = (target) => target != null && target != "";

export const apiFetchItems = async (filter) => {
  setItemsLoading(true);
  let filterDTO = structuredClone(filter);
  let requestData = { method: "POST" };
  if (filter == null) {
    filterDTO = { IsFilterOn: false };
  } else {
    const currencies = state.get().currencies.map((c) => {
      return {
        id: c.steamId,
        name: c.code,
      };
    });
    const regions = state.get().steamRegions.map((c) => {
      return {
        id: c.id,
        name: c.name,
      };
    });
    const regionVal = (
      regions.find((c) => c.name === filterDTO.steamCountryCodeId) || {}
    ).id;

    if (isNullOrEmpty(filterDTO.hierarchyParams_baseSteamCurrencyId)) {
      filterDTO.hierarchyParams_baseSteamCurrencyId = currencies.find(
        (c) => c.name === filterDTO.hierarchyParams_baseSteamCurrencyId
      ).id;
    }
    if (isNullOrEmpty(filterDTO.hierarchyParams_targetSteamCurrencyId)) {
      filterDTO.hierarchyParams_targetSteamCurrencyId = currencies.find(
        (c) => c.name === filterDTO.hierarchyParams_targetSteamCurrencyId
      ).id;
    }

    if (isNullOrEmpty(filterDTO.steamCountryCodeId)) {
      filterDTO.steamCountryCodeId = regionVal;
    }
    filterDTO.IsFilterOn = true;
  }

  requestData.body = mapToFormData(filterDTO);
  let res = await fetch("/items/list", requestData);
  setItemsLoading(false);
  setItems(await res.json());
};

export const setItemPrice = async (gpId, newPrice, itemId) => {
  let res = await apiSetItemPrice(gpId, newPrice);
  if (res.ok) {
    let item = await apiGetItem(itemId);
    setSelectedItem(item);
  }
};
export const apiSetItemPrice = async (gpId, newPrice) => {
  let res = await fetch(`/items/price/${gpId}/${newPrice}`, { method: "POST" });
  return res;
};

export const setItemPricePriority = async (gpId, itemId) => {
  let res = await apiSetItemPricePriority(gpId);
  if (res.ok) {
    let item = await apiGetItem(itemId);
    setSelectedItem(item);
  }
};

export const apiSetItemPricePriority = async (gpId) => {
  let res = await fetch(`/items/price/${gpId}/priority`, { method: "POST" });
  console.log(res);
  return res;
};

export const sortByPrice = async (items, price) => {
    return items.sort((a, b) => {
        if (a[price] < b[price]) {
            return -1;
        }
        if (a[price] > b[price]) {
            return 1;
        }
        return 0;
    });
};

export const apiFetchGameSessions = async (filter) => {
  let res = await fetch("/gamesessions/list", {
    method: "POST",
    body: mapToFormData({
      appId: filter.appId,
      gameName: filter.gameName,
      orderId: filter.orderId,
      profileStr: filter.profileStr,
      uniqueCodes: filter.uniqueCodes,
      comment: filter.comment,
      steamCurrencyId: filter.steamCurrencyId,
      page: filter.page,
      statusId: filter.statusId,
    }),
  });

  const json = await res.json();
  setStateProp("gameSessions", json.list);
  setStateProp("gameSessionsTotal", json.total);
};

export const apiFetchGameSessionsWithCurrentFilter = async () => {
  let filter = state.get().gameSessionsFilter;
  await apiFetchGameSessions(filter);
};

export const apiFetchGameSession = async (gsId) => {
  let res = await fetch(`/gamesessions/${gsId}`);

  if (res.ok) {
    const json = await res.json();
    return json;
  }
  return null;
};

export const apiFetchGameSessStatuses = async () => {
  let gameSessionsStatuses = state.get().gameSessionsStatuses;
  if (gameSessionsStatuses && Object.keys(gameSessionsStatuses).length > 0)
    return;

  let res = await fetch("/gamesessions/statuses");
  setStateProp("gameSessionsStatuses", await res.json());
};

export const apiFetchBots = async () => {
  let res = await fetch("/bots/list");
  setBots(await res.json());
};

export const apiFetchProxies = async () => {
  let res = await fetch("/proxy/list");
  setProxies(await res.json());
};

export const apiSetItemActiveStatus = async (ids) => {
  let idParam = "";
  if (ids && ids.length > 0) {
    idParam = ids.join(",");
  }

  if (!ids || ids.length > 1) setItemsLoading(true);

  let res = await fetch(`/items/SetActive?ids=${idParam}`);
  await apiFetchItems();
  if (!ids || ids.length > 1) setItemsLoading(false);
};

export const apiDeleteItem = async (id) => {
  let res = await fetch(`/items/Delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        items: value.items.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiBulkDeleteItem = async (Ids) => {
  let res = await fetch(`/items/bulk/delete`, {
    method: "POST",
    body: mapToFormData({
      Ids,
    }),
  });

  await apiFetchItems();
  return res.ok;
};

export const apiDeleteProxy = async (id) => {
  let res = await fetch(`/proxy/delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        proxies: value.proxies.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiDeleteProxyAll = async (id) => {
  let res = await fetch(`/proxy/delete/all`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        proxies: [],
      };
    });
  }
};

export const apiLoadNewProxy = async (data) => {
  let res = await fetch(`/proxy/load`, {
    method: "POST",
    body: mapToFormData(data),
  });
  if (!res.ok) {
    return;
  }
  toggleLoadProxiesModal(false);
  await apiFetchProxies();
};

export const apiEditBot = async (item) => {
  setStateProp("editBotResponse", {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/bots/add`, {
    method: "POST",
    body: mapToFormData(item),
  });

  let errors = [];
  if (res.ok) {
    toggleEditBotModal(false);
    await apiFetchBots();
  } else {
    if (res.status === 500) {
      errors.push("Произошла непредвиденная ошибка, проверьте консоль.");
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp("editBotResponse", {
    loading: false,
    errors: errors,
  });

  return res.ok;
};

export const apiDeleteBot = async (id) => {
  let res = await fetch(`/bots/Delete?id=${id}`);
  if (res.ok) {
    state.set((value) => {
      return {
        ...value,
        bots: value.bots.filter((i) => i.id !== id),
      };
    });
  }
};

export const apiBotSetIsOn = async (id, isOn) => {
  let res = await fetch(`/bots/setison`, {
    method: "POST",
    body: mapToFormData({
      botId: id,
      isOn: isOn,
    }),
  });

  //await apiFetchBots();
};

export const apiSaveBotRegionSettings = async (item) => {
  setStateProp("saveBotRegionSetResponse", {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/bots/regionsettings`, {
    method: "POST",
    body: mapToFormData(item),
  });

  let errors = [];
  if (res.ok) {
    toggleEditBotRegionSetModal(false);
    await apiFetchBots();
  } else {
    if (res.status === 500) {
      errors.push("Произошла непредвиденная ошибка, проверьте консоль.");
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp("saveBotRegionSetResponse", {
    loading: false,
    errors: errors,
  });

  return res.ok;
};

export const apiChangeItem = async (item) => {
  setItemsLoading(true);
  toggleEditItemModal(false);

  let res = await fetch(`/items/edit/${item.id}`, {
    method: "POST",
    body: mapToFormData(item),
  });

  if (res.ok) {
    await apiFetchItems();
  } else {
    toggleEditItemModal(true);
  }
  setItemsLoading(false);
};

export const apiCreateItem = async (item) => {
  setItemsLoading(true);
  toggleEditItemModal(false);

  let res = await fetch(`/items/add`, {
    method: "POST",
    body: mapToFormData(item),
  });

  if (res.ok) {
    await apiFetchItems();
  } else {
    toggleEditItemModal(true);
  }
  setItemsLoading(false);
};

export const setItemsLoading = (isOn) => {
  state.set((value) => {
    return {
      ...value,
      itemsLoading: isOn,
    };
  });
};

export const setItemInfoTemplatesLoading = (isOn) => {
  state.set((value) => {
    return {
      ...value,
      itemInfoTemplatesLoading: isOn,
    };
  });
};

export const setItemInfoTemplates = (itemInfoTemplates) => {
  state.set((value) => {
    return {
      ...value,
      itemInfoTemplates: itemInfoTemplates,
    };
  });
};

export const toggleBulkEditPercentModal = (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      bulkEditPercentModalIsOpen: isOpen,
    };
  });
};

export const toggleItemMainInfoModal = (isOpen) => {
  apiFetchItemInfoTemplates(0).then(() => {
    state.set((value) => {
      return {
        ...value,
        editItemMainInfoModalIsOpen: isOpen,
      };
    });
  })
};

export const toggleItemAdditionalInfoModal = (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editItemAdditionalInfoModalIsOpen: isOpen,
    };
  });
};

export const toggleLoadProxiesModal = (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      loadProxiesModalIsOpen: isOpen,
    };
  });
};

export const toggleAddGameSesCommentModal = (isOpen) => {
  setStateProp("addGameSesCommentIsOpen", isOpen);
};

export const toggleViewStatusHistoryModal = (isOpen) => {
  setStateProp("statusHistoryModalIsOpen", isOpen);
};

export const toggleDigisellerEditModal = async (isOpen) => {
  if (isOpen === true) {
    await apiGetCurrentUser();
  }

  state.set((value) => {
    return {
      ...value,
      digisellerEditModalIsOpen: isOpen,
    };
  });
};

export const toggleExchangeRatesModal = async (isOpen) => {
  if (isOpen === true) {
    await apiGetExchageRates();
  }

  state.set((value) => {
    return {
      ...value,
      exchangeRatesModalIsOpen: isOpen,
    };
  });
};

export const toggleBotDetailsModal = (isOpen) => {
  setStateProp("botDetailsModalIsOpen", isOpen);
};

export const toggleChangePasswordModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      changePasswordModalIsOpen: isOpen,
    };
  });
};

export const toggleEditBotModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editBotModalIsOpen: isOpen,
    };
  });
};

export const toggleEditBotRegionSetModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      botRegionSetEditModalIsOpen: isOpen,
    };
  });
};

export const toggleEditOrderModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editOrderModalIsOpen: isOpen,
    };
  });
};

export const toggleFilterOrdersModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      filterOrdersModalIsOpen: isOpen,
    };
  });
};

export const toggleFilterProductsModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      filterProductsModalIsOpen: isOpen,
    };
  });
};

export const toggleEditItemModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editItemModalIsOpen: isOpen,
    };
  });
};

export const toggleEditItemMainInfoModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editItemMainInfoModalIsOpen: isOpen,
    };
  });
};

export const toggleEditItemAdditionalInfoModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      editItemAdditionalInfoModalIsOpen: isOpen,
    };
  });
};


export const toggleOrderCreationInfoModal = async (isOpen) => {
  state.set((value) => {
    return {
      ...value,
      orderCreationInfoIsOpen: isOpen,
    };
  });
};

export const apiChangeItemBulk = async (SteamPercent, IncreaseDecreaseOperator, IncreaseDecreasePercent, Ids) => {
  setItemsLoading(true);
  setStateProp("changeItemBulkResponse", { loading: true });
  let res = await fetch(`/items/bulk/change`, {
    method: "POST",
    body: mapToFormData({
      SteamPercent,
      IncreaseDecreaseOperator,
      IncreaseDecreasePercent,
      Ids,
    }),
  });
  setStateProp("changeItemBulkResponse", { loading: false });
  await apiFetchItems();
};

export const apiChangeDigisellerData = async (data) => {
  let res = await fetch(`/user/edit/digiseller`, {
    method: "POST",
    body: mapToFormData(data),
  });
};

export const apiChangeUserPassword = async (data) => {
  let res = await fetch(`/user/password`, {
    method: "POST",
    body: mapToFormData(data),
  });

  toggleChangePasswordModal(!res.ok);
};

export const apiGetCurrentUser = async () => {
  let res = await fetch(`/user`);
  let data = await res.json();
  state.set((value) => {
    return {
      ...value,
      user: data,
    };
  });
};

export const apiGetExchageRates = async () => {
  let res = await fetch(`/exchangerates/list`);
  let data = await res.json();
  state.set((value) => {
    return {
      ...value,
      exchageRates: data,
    };
  });
};

export const apiUpdateExchangeDataManual = async (data) => {
  //let res = await
  fetch(`/exchangerates/update`, {
    method: "POST",
    body: mapToFormData(data),
  });

  //toggleExchangeRatesModal(!res.ok);
  toggleExchangeRatesModal(false);
};

export const apiGetCurrencies = async () => {
  let currencies = state.get().currencies;
  if (currencies && currencies.length > 0) return;

  let res = await fetch(`/exchangerates/list`);
  let data = await res.json();

  currencies = data.currencies.map((c) => {
    return {
      code: c.code,
      steamId: c.steamId,
      steamSymbol: c.steamSymbol,
    };
  });

  state.set((value) => {
    return {
      ...value,
      currencies: currencies,
    };
  });
};

export const apiGetItem = async (id) => {
  let res = await fetch(`/items/${id}/info`);
  let data = await res.json();

  return data;
};

export const apiGetSteamRegions = async () => {
  let steamRegions = state.get().steamRegions;
  if (steamRegions && steamRegions.length > 0) return;

  let res = await fetch(`/dict/regions`);
  let data = await res.json();

  steamRegions = data;
  setStateProp("steamRegions", steamRegions);
};

export const apiSetGameSessionStatus = async (gSesId, statusId) => {
  const { gameSessionsFilter } = state.get();

  let res = await fetch(`/gamesessions/setstatus`, {
    method: "POST",
    body: mapToFormData({
      gameSessionId: gSesId,
      statusId: statusId,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const apiResetGameSession = async (gSesId) => {
  const { gameSessionsFilter } = state.get();

  let res = await fetch(`/gamesessions/reset`, {
    method: "POST",
    body: mapToFormData({
      gameSessionId: gSesId,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const apiAddCommentGameSession = async (gSesId, comment) => {
  const { gameSessionsFilter } = state.get();
  toggleAddGameSesCommentModal(false);

  let res = await fetch(`/gamesessions/comment`, {
    method: "POST",
    body: mapToFormData({
      gameSessionId: gSesId,
      comment: comment,
    }),
  });

  apiFetchGameSessions(gameSessionsFilter);
};

export const updateGameSessionsFilter = async (newData) => {
  const { gameSessionsFilter } = state.get();
  let newFilter = {
    ...gameSessionsFilter,
    ...newData,
  };
  console.log("new f", newFilter);

  setStateProp("gameSessionsFilter", newFilter);
  await apiFetchGameSessions(newFilter);
};

export const updateProductsFilter = async (newData) => {
  const { productsFilter } = state.get();
  let newFilter = {
    ...productsFilter,
    ...newData,
  };
  console.log("new f", newFilter);

  setStateProp("productsFilter", newFilter);
  await apiFetchItems(newFilter);
};

export const apiAddGameSession = async (data) => {
  setStateProp("editOrderResponse", {
    loading: true,
    errors: [],
  });

  let res = await fetch(`/gamesession`, {
    method: "POST",
    body: mapToFormData(data),
  });

  let errors = [];
  if (res.ok) {
    const { gameSessionsFilter } = state.get();
    await apiFetchGameSessions(gameSessionsFilter);
    toggleEditOrderModal(false);
  } else {
    if (res.status === 500) {
      errors.push("Произошла непредвиденная ошибка, проверьте консоль.");
    } else {
      errors = (await res.json()).errors;
    }
  }

  setStateProp("editOrderResponse", {
    loading: false,
    errors: errors,
  });

  if (res.ok) {
    var newUniqueCodes = await res.json();
    console.log(newUniqueCodes);
    state.set((value) => {
      return {
        ...value,
        newUniqueCodes: newUniqueCodes,
      };
    });
    toggleOrderCreationInfoModal(true);
  }
};

export const apiFetchItemInfoTemplates = async (userId) => {
  setItemInfoTemplatesLoading(true);

  let result = await fetch(`/iteminfotemplate?userId=${userId}`);

  if (result.ok) {
    const json = await result.json();
    setItemInfoTemplates(json);
  }

  setItemInfoTemplatesLoading(false);
};

export const apiFetchItemInfoTemplateValues = async (itemInfoTemplateId) => {
  setItemInfoTemplatesLoading(true);

  let result = await fetch(`/iteminfotemplatevalue/${itemInfoTemplateId}`);

  if (result.ok) {
    setItemInfoTemplatesLoading(false);
    return await result.json();
  }

  setItemInfoTemplatesLoading(false);
  return null;
};

export const apiCreateItemInfoTemplate = async (itemInfoTemplateValues) => {
  setItemInfoTemplatesLoading(true);

  const headers = new Headers();
  headers.append("Content-Type", "application/json");
  headers.append("Content-Length", JSON.stringify(itemInfoTemplateValues).length);

  const options = {
    method: "POST",
    headers: headers,
    body: JSON.stringify(itemInfoTemplateValues)
  }

  let res = await fetch(`/iteminfotemplate`, options);

  if (res.ok) {
    await apiFetchItemInfoTemplates(0);
  }

  setItemInfoTemplatesLoading(false);
};

export const apiDeleteItemInfoTemplate = async (itemInfoTemplateId) => {
  setItemInfoTemplatesLoading(true);

  const options = {
    method: "DELETE"
  }

  let res = await fetch(`/iteminfotemplate/${itemInfoTemplateId}`, options);

  if (res.ok) {
    await apiFetchItemInfoTemplates(0);
  }

  setItemInfoTemplatesLoading(false);
};

export const apiUpdateItemInfoes = async (itemInfoesValues) => {
  toggleItemMainInfoModal(false);
  toggleItemAdditionalInfoModal(false);
  setItemsLoading(true);
  setStateProp("changeItemBulkResponse", { loadingItemInfo: true });
  try {
    const headers = new Headers();
    headers.append("Content-Type", "application/json");
    headers.append("Content-Length", JSON.stringify(itemInfoesValues).length);

    const options = {
      method: "PATCH",
      headers: headers,
      body: JSON.stringify(itemInfoesValues)
    }

    let res = await fetch(`/iteminfo`, options);
    if (res.ok) {
      await apiFetchItems();
    }
  }
  finally {
    setStateProp("changeItemBulkResponse", { loadingItemInfo: false });
    setItemsLoading(false);
  }
};

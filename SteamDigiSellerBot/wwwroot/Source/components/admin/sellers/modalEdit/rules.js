export const paramToName = new Map([
  ['permissionDigisellerItems', 'Товары Digiseller'],
  ['permissionKFGItems', 'Товары KFG'],
  ['permissionFuryPayItems', 'Товары FunPay'],
  ['permissionItemsHierarchy', 'Иерархия товаров + фильтр'],
  ['permissionOneTimeBots', 'Одноразовые боты'],
  ['permissionOrderSessionCreation', 'Создание сессий заказов'],
  ['permissionItemsMultiregion', 'Мульти-регион товаров'],
  ['permissionDirectBotsDeposit', 'Прямое пополнение ботов'],
  ['permissionBotsLimitsParsing', 'Отображение лимитов ботов'],
  ['permissionDigisellerItemsGeneration', 'Генерация товара Digiseller'],
  ['permissionSteamPointsAutoDelivery', 'Автодоставка Steam Points'],
]);

export const initPermissions = new Map([
  ['permissionDigisellerItems', false],
  ['permissionKFGItems', false],
  ['permissionFuryPayItems', false],
  ['permissionItemsHierarchy', false],
  ['permissionOneTimeBots', false],
  ['permissionOrderSessionCreation', false],
  ['permissionItemsMultiregion', false],
  ['permissionDirectBotsDeposit', false],
  ['permissionDirectBotsDepositPercent', '5'],
  ['permissionBotsLimitsParsing', false],
  ['permissionDigisellerItemsGeneration', false],
  ['permissionSteamPointsAutoDelivery', false]])

export const permissionsGetValueActions = new Map([
  ['permissionDigisellerItems', (elem) => elem.permissionDigisellerItems],
  ['permissionKFGItems', (elem) => elem.permissionKFGItems],
  ['permissionFuryPayItems', (elem) => elem.permissionFuryPayItems],
  ['permissionItemsHierarchy', (elem) => elem.permissionItemsHierarchy],
  ['permissionOneTimeBots', (elem) => elem.permissionOneTimeBots],
  ['permissionOrderSessionCreation', (elem) => elem.permissionOrderSessionCreation],
  ['permissionItemsMultiregion', (elem) => elem.permissionItemsMultiregion],
  ['permissionDirectBotsDeposit', (elem) => elem.permissionDirectBotsDeposit],
  ['permissionDirectBotsDepositPercent', (elem) => elem.permissionDirectBotsDepositPercent],
  ['permissionBotsLimitsParsing', (elem) => elem.permissionBotsLimitsParsing],
  ['permissionDigisellerItemsGeneration', (elem) => elem.permissionDigisellerItemsGeneration],
  ['permissionSteamPointsAutoDelivery', (elem) => elem.permissionSteamPointsAutoDelivery]])

export const permissionsSetValueActions = new Map([
  ['permissionDigisellerItems', (elem, value) => elem.permissionDigisellerItems = value],
  ['permissionKFGItems', (elem, value) => elem.permissionKFGItems = value],
  ['permissionFuryPayItems', (elem, value) => elem.permissionFuryPayItems = value],
  ['permissionItemsHierarchy', (elem, value) => elem.permissionItemsHierarchy = value],
  ['permissionOneTimeBots', (elem, value) => elem.permissionOneTimeBots = value],
  ['permissionOrderSessionCreation', (elem, value) => elem.permissionOrderSessionCreation = value],
  ['permissionItemsMultiregion', (elem, value) => elem.permissionItemsMultiregion = value],
  ['permissionDirectBotsDeposit', (elem, value) => elem.permissionDirectBotsDeposit = value],
  ['permissionDirectBotsDepositPercent', (elem, value) => elem.permissionDirectBotsDepositPercent = value],
  ['permissionBotsLimitsParsing', (elem, value) => elem.permissionBotsLimitsParsing = value],
  ['permissionDigisellerItemsGeneration', (elem, value) => elem.permissionDigisellerItemsGeneration = value],
  ['permissionSteamPointsAutoDelivery', (elem, value) => elem.permissionSteamPointsAutoDelivery = value]])

  export const initial = {
    id: 0,
    login: '',
    password: '',
    userId: '',
    rentDays: '-',
    itemsLimit: 0,
    blocked: false,
    permissionDigisellerItems: false,
    permissionKFGItems: false,
    permissionFuryPayItems: false,
    permissionItemsHierarchy: false,
    permissionOneTimeBots: false,
    permissionOrderSessionCreation: false,
    permissionItemsMultiregion: false,
    permissionDirectBotsDeposit: false,
    permissionBotsLimitsParsing: false,
    permissionDigisellerItemsGeneration: false,
    permissionSteamPointsAutoDelivery: false
  };
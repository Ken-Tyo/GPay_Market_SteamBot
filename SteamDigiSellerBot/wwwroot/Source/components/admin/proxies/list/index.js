import React, { useEffect, useState } from 'react';
import { styled } from '@mui/material/styles';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell, { tableCellClasses } from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import IconButton from '../../../shared/iconButton';
import css from './styles.scss';
import trash from '../../../../icons/trash.svg';
import addItem from '../../../../icons/additem.svg';
import AddProxiesModal from '../../../shared/modalSaveText';
import {
  state,
  apiDeleteProxy,
  apiLoadNewProxy,
  toggleLoadProxiesModal,
} from '../../../../containers/admin/state';

const StyledTableCell = styled(TableCell)(({ theme }) => ({
  [`&.${tableCellClasses.head}`]: {
    backgroundColor: '#532569',
    color: theme.palette.common.white,
    fontSize: 20,
    lineHeight: '20px',
  },
  [`&.${tableCellClasses.body}`]: {},

  '&:last-child': {
    borderRadius: '0px 34px 0px 0px',
  },

  '&:first-child': {
    borderRadius: '34px 0px 0px 0px',
  },
}));

const StyledTableRow = styled(TableRow)(({ theme }) => ({
  '&:last-child td, &:last-child th': {
    border: 0,
  },
}));

const proxies = () => {
  const { proxies, loadProxiesModalIsOpen } = state.use();

  return (
    <div className={css.wrapper}>
      <TableContainer className={css.listHeadContainer}>
        <Table stickyHeader aria-label="sticky table" className={css.list}>
          <TableHead className={css.listHeader}>
            <TableRow>
              <StyledTableCell align="center" className={css.listHeaderItem}>
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    marginLeft: '100px',
                  }}
                >
                  <div>Прокси ({proxies.length})</div>
                  <img
                    src={addItem}
                    style={{ marginLeft: '10px', cursor: 'pointer' }}
                    onClick={async () => {
                      toggleLoadProxiesModal(!loadProxiesModalIsOpen);
                    }}
                  />
                </div>
              </StyledTableCell>
              <StyledTableCell align="center" className={css.listHeaderItem}>
                Опции
              </StyledTableCell>
            </TableRow>
          </TableHead>
        </Table>
      </TableContainer>
      <TableContainer className={css.listContainer}>
        <Table className={css.list}>
          <TableBody className={css.listItem}>
            {proxies.map((i) => {
              let name = `${i.host}:${i.port}:${i.userName}:${i.password}`;
              return (
                <StyledTableRow key={i.id}>
                  <TableCell>
                    <div className={css.name}>{name}</div>
                  </TableCell>
                  <TableCell>
                    <div className={css.buttons}>
                      <div className={css.btnWrapper}>
                        <IconButton
                          icon={trash}
                          onClick={() => {
                            apiDeleteProxy(i.id);
                          }}
                        />
                      </div>
                    </div>
                  </TableCell>
                </StyledTableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>

      <AddProxiesModal
        title={'Добавить прокси'}
        placeholder={
          'Введите сюда список общих прокси на для использования под парсинг распродаж, подключение к API digiseller и т.д. Общие прокси в работе с ботами использоваться не будут. Прокси нужно вводить списком через Enter'
        }
        isOpen={loadProxiesModalIsOpen}
        onSave={{
          label: 'Импортировать',
          action: (val) => {
            apiLoadNewProxy({ proxies: val });
          },
        }}
        onCancel={{
          label: 'Отмена',
          action: () => {
            toggleLoadProxiesModal(false);
          },
        }}
      />
    </div>
  );
};

export default proxies;

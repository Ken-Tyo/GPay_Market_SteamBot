import React, { useState } from 'react';
import PageHeader from '../pageHeader';
import SellersList from './list';
import EditSellerModal from '../sellers/modalEdit';
import Section from '../section';
import css from './styles.scss';
import {
  state
} from '../../../containers/admin/state';


const Sellers = () => {
  const {
    editSellerModalIsOpen,
    editSellerResponse
  } = state.use();

  const [isEditSellerModalOpen, setIsEditSellerModalOpen] = useState(false)
  const [user, setUser] = useState()

  return (
    <div className={css.wrapper}>
      <Section className={css.section}>
        <Section className={css.titleSection} height={40}>
          <div className={css.title}>Продавцы</div>
        </Section>
        <div className={css.subTitle}>
          <div className={css.usersHeader}>
            <div className={css.usersTitle}>
              <div>Управление списком продавцов</div>

              <svg width="18" height="18" viewBox="0 0 18 18" fill="none" xmlns="http://www.w3.org/2000/svg" onClick={() => setIsEditSellerModalOpen(true)}>
                <path d="M13 9H5M9 13V5" stroke="#B93ED8" stroke-linecap="square" />
                <path d="M9 17C13.4183 17 17 13.4183 17 9C17 4.58172 13.4183 1 9 1C4.58172 1 1 4.58172 1 9C1 13.4183 4.58172 17 9 17Z" stroke="#B93ED8" stroke-linecap="square" />
              </svg>
            </div>

            <div className={css.notification}>
              <svg width="22" height="25" viewBox="0 0 22 25" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M21.8019 18.0226C21.0545 16.7437 20.1637 14.3304 20.1637 10.0962V9.24319C20.1637 4.18361 16.0843 0.0372126 11.0699 0.000240253C11.0465 0.00012006 11.0233 0 10.9999 0C8.56657 0.00330375 6.23421 0.967542 4.5159 2.6806C2.79758 4.39366 1.83406 6.71523 1.83728 9.13461V10.0962C1.83728 14.3302 0.945992 16.7435 0.198244 18.0224C0.0695243 18.2413 0.00114877 18.4902 1.43494e-05 18.7438C-0.00112007 18.9974 0.0650266 19.2468 0.191783 19.4668C0.318539 19.6869 0.501422 19.8699 0.721986 19.9972C0.94255 20.1246 1.193 20.1919 1.44806 20.1923H6.64814V20.6731C6.64814 21.8206 7.10665 22.9212 7.92278 23.7327C8.73892 24.5441 9.84583 25 11 25C12.1542 25 13.2611 24.5441 14.0773 23.7327C14.8934 22.9212 15.3519 21.8206 15.3519 20.6731V20.1923H20.552C20.807 20.1919 21.0574 20.1246 21.278 19.9972C21.4985 19.8699 21.6814 19.687 21.8081 19.4669C21.9349 19.2469 22.0011 18.9975 22 18.744C21.9989 18.4904 21.9306 18.2416 21.8019 18.0226ZM14.3848 20.6731C14.3848 21.5656 14.0282 22.4216 13.3934 23.0528C12.7587 23.6839 11.8977 24.0385 11 24.0385C10.1023 24.0385 9.24138 23.6839 8.60661 23.0528C7.97184 22.4216 7.61523 21.5656 7.61523 20.6731V20.1923H14.3848V20.6731ZM20.9674 18.9918C20.926 19.065 20.8657 19.1258 20.7926 19.1678C20.7195 19.2099 20.6364 19.2316 20.552 19.2308H1.44806C1.36362 19.2316 1.28049 19.2099 1.2074 19.1678C1.13431 19.1258 1.07395 19.0649 1.03264 18.9917C0.989029 18.918 0.966171 18.834 0.966462 18.7484C0.966753 18.6629 0.990181 18.579 1.03429 18.5056C1.84177 17.1245 2.80436 14.5445 2.80436 10.0962V9.13462C2.80114 6.97021 3.6628 4.89318 5.1998 3.36043C6.73681 1.82768 8.82325 0.964781 11.0001 0.961548C11.0209 0.961548 11.042 0.961668 11.0628 0.961788C15.5477 0.994888 19.1966 4.70993 19.1966 9.2432V10.0962C19.1966 14.5445 20.1586 17.1246 20.9656 18.5058C21.0098 18.5792 21.0332 18.663 21.0335 18.7485C21.0338 18.8341 21.011 18.9181 20.9674 18.9918Z" fill="white" />
              </svg>
            </div>

          </div>
        </div>
      </Section>

      <div className={css.content}>
        <EditSellerModal
          isOpen={isEditSellerModalOpen}
          onClose={(val) => {
            setUser(val)
            setIsEditSellerModalOpen(false);
          }}
        />

        <SellersList user={user} />
      </div>
    </div>
  );
};

export default Sellers;

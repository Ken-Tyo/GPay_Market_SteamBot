import React, { useEffect, useState, useRef } from 'react';
import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import arrowLeft from '../../../icons/arrowLeft.svg';
import arrowRight from '../../../icons/arrowRight.svg';
import css from './styles.scss';
//import { state, setActiveMenuLink } from '../../../containers/admin/state';

const MenuItem = ({
  name,
  icon,
  subMenu,
  subMenuStyle,
  onClick,
  onClickOutside,
  isOpen,
  url,
}) => {
  //const s = state.use();
  const menuRef = useRef(null);
  const subMenuRef = useRef(null);
  const navigate = useNavigate();
  const subMenuExist = subMenu && subMenu.length > 0;
  const { pathname } = useLocation();

  const handleClick = (e) => {
    if (
      menuRef.current &&
      !menuRef.current.contains(e.target) &&
      subMenuRef.current &&
      !subMenuRef.current.contains(e.target)
    ) {
      onClickOutside();
    }
  };

  useEffect(() => {
    document.addEventListener('click', handleClick);

    return function () {
      document.removeEventListener('click', handleClick);
    };
  });

  const isActiveLocation = () => {
    return (
      isOpen ||
      (url && url.includes(pathname)) ||
      (subMenu &&
        subMenu.map((si) => si.url).filter((i) => i && i.includes(pathname))
          .length > 0)
    );
  };

  return (
    <div
      ref={menuRef}
      className={
        css.menuItem +
        ' ' +
        //(isOpen || s.activeMenuLink === name ? css.isOpen : '')
        (isActiveLocation() ? css.isOpen : '')
      }
      onClick={() => {
        onClick();
        //if (!subMenuExist) setActiveMenuLink(name);
      }}
    >
      <div style={{ display: 'flex' }}>
        <div className={css.icon}>
          <img src={icon}></img>
        </div>
        <div>{name}</div>
      </div>
      {subMenuExist && (
        <>
          <div>
            {isOpen && <img className={css.arrowIcon} src={arrowLeft}></img>}
            {!isOpen && <img className={css.arrowIcon} src={arrowRight}></img>}
          </div>

          {isOpen && (
            <div className={css.subMenu} style={subMenuStyle} ref={subMenuRef}>
              {subMenu.map((i) => {
                return (
                  <div
                    className={css.subMenuItem}
                    onClick={() => {
                      //setActiveMenuLink(name);
                      if (i.action) {
                        i.action();
                      }
                      if (i.url) navigate(i.url);
                    }}
                  >
                    {i.name}
                  </div>
                );
              })}
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default MenuItem;

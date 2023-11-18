import React from 'react';
import Section from '../section';
import css from './styles.scss';

const pageHeader = ({ title, subTitle }) => {
  return (
    <Section className={css.section}>
      <Section className={css.titleSection} height={40} width={203}>
        <div className={css.title}>{title}</div>
      </Section>
      <div className={css.subTitle}>
        <div>{subTitle}</div>
      </div>
    </Section>
  );
};

export default pageHeader;

# Баг-репорти: LiteCart

Дефекти, знайдені під час ручного тестування інтернет-магазину LiteCart на локальному стенді.
Кожен тікет відтворено вручну; Expected спирається на поведінку системи в сусідньому стані, а не на код.

| Тікет | Що зламано | Severity |
|---|---|---|
| LC-1 | Fatal error на картці акційного товару й в адмінці, коли `intl` вимкнене | Blocker |
| LC-9 | Warning `display_lowest_price_30_days` на картці товару | Minor |
| LC-10 | Fatal error в адмінці, коли часовий пояс магазину — `Europe/Kyiv` | Blocker |
| LC-11 | Stock Status «-1 pcs», коли зарезервовано більше, ніж є на складі | Major |
| LC-12 | Алерт показує залишок 30 замість 0, коли весь залишок уже в кошику | Minor |

## Environment (спільне для всіх тікетів)
| | |
|---|---|
| Продукт | LiteCart 2.7.0 (`PLATFORM_VERSION`), локальна інсталяція |
| Стек | XAMPP: Apache, PHP 8.2.12 (ZTS x64), MariaDB 10.4.32 |
| ОС | Windows 11 Home |
| Браузер | Chrome 153.0.8010.36 (64-bit) |
| URL | http://localhost/litecart/ |

## LC-1 · Картка акційного товару й адмінка: Fatal error при форматуванні дати, коли PHP-розширення intl вимкнене

| | |
|---|---|
| **Severity** | **Blocker** |
| **Priority** | *(ставить PM/лід)* |
| **Відтворюваність** | 100 %, щоразу |
| **Статус** | Open |

### Environment
Спільне (див. вище), плюс:
- PHP-розширення `intl` вимкнене: `;extension=intl` у `php.ini`
- http://localhost/dashboard/phpinfo.php — блок `intl` відсутній

### Preconditions
**Сценарій A** — будь-який користувач.
Наявний акційний товар з наявним кінцевим терміном акції (31.12.2026 р.).
Наприклад - http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/yellow-duck-p-1
http://localhost/litecart/admin/?app=catalog&doc=edit_product&category_id=1&product_id=1#tab-prices  -> Campaigns -> End Date (31.12.2026) -> Save

### Steps

**Сценарій A — сторінка акційного товару:**
1. Відкрити картку акційного товару http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/yellow-duck-p-1

**Сценарій B — адмінка:**
1. Відкрити http://localhost/litecart/admin/
2. Увійти з обліковими даними адміністратора
3. Подивитись сторінку

### Actual Result

**A.** Сторінка рендериться частково. Наявні блоки: Категорії, Іконка, Назва, Опис, Вибір розміру, Ціна, Найнижча ціна за останні 30 днів.
Після чого відображається текст помилки і сторінка обривається.
Термін акції та кнопка «Add to cart» не відображаються — купити товар неможливо.


**B.** Невідрендерена (пуста біла) сторінка з текстом помилки вгорі.

Текст помилки однаковий в обох сценаріях:

```
Warning: You need the PHP Intl extension enabled to format dates in ~/includes/library/lib_language.inc.php on line 313
Fatal error: date(): Argument #2 ($timestamp) must be of type ?int, DateTime given in ~/includes/library/lib_language.inc.php on line 314
```

### Expected Result
**A.** Сторінка рендериться повністю. Відображаються термін дії акції, поле вводу кількості та кнопка «Add to cart».
Підстава: той самий товар із тією ж датою при увімкненому `intl` відображається повністю, з терміном дії акції.

**B.** Після логіну відкривається дашборд адмінки.
Підстава: базова функція продукту; без адмінки неможливо керувати замовленнями, статусами й складом.

### Notes / Localization
- Без дати закінчення акції картка товару відображається коректно.
---

## LC-9 · Картка товару: Warning «Unsupported settings key (display_lowest_price_30_days)»

| |                    |
|---|--------------------|
| **Severity** | Minor              |
| **Priority** | *(ставить PM/лід)* |
| **Відтворюваність** | 100 %              |
| **Статус** | Open               |

### Environment
Спільне (див. вище). `display_errors = On` — Warning видно на сторінці
### Preconditions
Будь-який товар у каталозі, будь-який користувач.

### Steps
1. Відкрити картку будь-якого товару, наприклад Red Duck: http://localhost/litecart/rubber-ducks-c-1/red-duck-p-3

### Actual Result
Угорі сторінки (перед контентом) виводиться:

```
Warning: Unsupported settings key (display_lowest_price_30_days)
  in ~/includes/library/lib_settings.inc.php on line 66
```

Решта сторінки працює нормально.
На головній сторінці, у списку категорії та в кошику Warning відсутній.

### Expected Result
Картка товару відкривається без Warning.
Підстава: поведінка решти сторінок магазину (головна, категорія, кошик).

### Notes / Localization
- У базі ключ відсутній:
  ```sql
  SELECT id, `key`, value FROM lc_settings
  WHERE `key` LIKE '%lowest%' OR `key` LIKE '%30%';   -- 0 рядків, усього ключів 91
  ```
---

## LC-11 · Картка товару: Stock Status показує «-1 pcs», коли зарезервовано більше, ніж є на складі

| |                                                                                                                                             |
|---|---------------------------------------------------------------------------------------------------------------------------------------------|
| **Severity** | Major — функція працює, але показує покупцю неможливе значення; обхід є (не переводити замовлення з кількістю понад залишок у Ready-статус) |
| **Priority** | *(ставить PM/лід)*                                                                                                                          |
| **Відтворюваність** | 100 %, доки виконуються Preconditions                                                                                                       |
| **Статус** | Open                                                                                                                                        |

### Environment
Спільне (див. шапку). Специфічне:
- `display_stock_count` = 1 (увімкнений) — на картках товару видно «Stock Status: N pcs».

### Preconditions
- Товар Green Duck (http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/green-duck-p-2) (`sku = RD002`): `lc_products.quantity = 30`, sold_out_status_id = 2 (orderable = 1)
- Замовлення **№6**: одна позиція — 31 × Green Duck, створене через UI магазину 2026-09-15, поточний статус «Awaiting payment».
- Інших замовлень на Green Duck у статусах з `stock_action = 'reserve'` (Ready, Delayed, Returning) немає:
  ```sql
  SELECT o.id, o.order_status_id, oi.quantity
  FROM lc_orders_items oi JOIN lc_orders o ON o.id = oi.order_id
  WHERE oi.sku = 'RD002';
  ```

### Steps
1. Адмінка → Orders → замовлення ID = 6 → змінити статус на Ready → Save.
2. У магазині відкрити картку товару - http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/green-duck-p-2
3. Подивитись блок Stock Status

### Actual Result
Відображається від'ємний залишок товару (Stock Status: -1 pcs). Кнопка додати до кошику та поле вводу кількості доступні та активні.

У базі `lc_products.quantity` = 30.

### Expected Result
Відображається "Stock Status: Temporarily Sold Out". Кнопка «Add to cart» і поле кількості лишаються активними.
Так відображається Blue Duck при quantity = 0 - http://localhost/litecart/rubber-ducks-c-1/blue-duck-p-4

### Notes
Delayed / Returning — не перевірено

---

## LC-12 · Картка товару: алерт відмови показує залишок 30 замість 0, коли весь залишок уже в кошику
| |                                 |
|---|---------------------------------|
| **Severity** | Minor                           |
| **Priority** | *(ставить PM/лід)*              |
| **Відтворюваність** | 100 %, картка товару - Red Duck |
| **Статус** | Open                            |

### Environment
Спільне (див. вище).

### Preconditions
Порожній кошик.
Товар - Red Duck: http://localhost/litecart/rubber-ducks-c-1/red-duck-p-3, (`sku = RD003`): `lc_products.quantity = 30`, sold_out_status_id = 1 (orderable = 0)
```sql
    SELECT sku, sold_out_status_id, quantity FROM lc_products WHERE sku = 'RD003' AND quantity = 30 AND sold_out_status_id = 1;
```

### Steps
1. Відкрити картку товару http://localhost/litecart/rubber-ducks-c-1/red-duck-p-3
2. У полі Quantity ввести 30 → Add to cart
3. У полі Quantity ввести 1 → Add to cart
4. Подивитись браузерний альорт

### Actual Result
Товар не додано, у кошику лишилось 30.
В тексті альорта вказано - "There are only 30 remaining products available for purchase", але всі 30 одиниць вже були додані в кошик.

### Expected Result
«There are only 0 remaining products available for purchase»

Підстава: при додаванні 31 у порожній кошик алерт показує коректне 30 — система рахує доступний залишок.
При 30 у кошику доступно 0.

### Notes / Localization
Stock Status: 30 pcs — коректно, кошик не резервує товар.
- 31 у порожній кошик → алерт «30» (коректно)
- 30 у порожній кошик → прийнято
- 30, потім +1 → алерт «30» (має бути 0)
---

## LC-10 · Адмінка: після логіну Fatal error, коли часовий пояс магазину — Europe/Kyiv
| | |
|---|---|
| **Severity** | **Blocker** — адмінка недоступна |
| **Priority** | *(ставить PM/лід)* |
| **Відтворюваність** | 100 %, щоразу |
| **Статус** | Open |

### Environment
Спільне (див. шапку) +
http://localhost/dashboard/phpinfo.php
ICU version - 71.1
ICU TZData version - 2022a
Timezone Database Version - 2023.3

intl увімкнене.

### Preconditions
Часовий пояс магазину — `Europe/Kyiv`

```sql
  SELECT id, value FROM lc_settings WHERE id = 11;   -- Europe/Kyiv
```
### Steps

1. Відкрити `http://localhost/litecart/admin/`
2. Увійти з обліковими даними адміністратора

### Actual Result
Замість дашборду відображається біла сторінка з помилкою в верху :

````
Fatal error: datefmt_create: time zone id 'Europe/Kyiv' extracted from ext/date DateTimeZone not recognized:
U_ILLEGAL_ARGUMENT_ERROR in ~/includes/library/lib_language.inc.php on line 365
````
Адмінка недоступна повністю.

### Expected Result
Після логіну відкривається сторінка дашборду - як наприклад після зміни на стару назву часового поясу

```
UPDATE lc_settings SET value = 'Europe/Kiev' WHERE id = 11;
```

### Notes / Localization
Обхід - 
```
UPDATE lc_settings SET value = 'Europe/Kiev' WHERE id = 11;
```
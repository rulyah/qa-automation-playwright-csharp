# Тест-кейси: LiteCart, кошик

- [TC-CART-001](#tc-cart-001)
- [TC-CART-002](#tc-cart-002)
- [Чек-лист «Кошик»](#чек-лист-кошик)

<a id="tc-cart-001"></a>
## TC-CART-001 · Кошик: гість додає Green Duck у кількості 1 до порожнього кошика, на іконці кошика відображається 1 товар

| | |
|---|---|
| **Priority** | High |
| **Type** | Positive |

**Preconditions**

- гість (не залогінений, нове вікно інкогніто), порожній кошик
- cookies прийнято (кнопка в банері) → оновити сторінку (F5)
- відкритий DevTools → Network
- `<cart_uid>` = значення куки `cart[uid]` (DevTools → Application → Cookies → http://localhost)
- Green Duck (id: 2, quantity: 30, товар без опцій (розмірів)):

  ```sql
  SELECT * FROM `lc_products` WHERE id = 2;
  ```

- у `lc_cart_items` немає рядків за `<cart_uid>`:

  ```sql
  SELECT * FROM `lc_cart_items` WHERE cart_uid = '<cart_uid>';
  ```

**Steps / Expected**

1. Відкрити картку товару http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/green-duck-p-2
   - сторінка відкрилась, відображається товар Green Duck, у полі кількості значення 1, наявна кнопка Add to cart
2. Натиснути кнопку Add to cart
   - лічильник кошика в шапці = 1
   - Network: POST з `product_id = 2`, `quantity = 1` → 302 → GET → 200. GET `ajax/cart.json` → у тілі 1 item з `product_id = 2`, `quantity = 1`, відсутній `alert`
   - у БД з'являється 1 рядок: `product_id = 2`, `quantity = 1`, `customer_id = 0`:

     ```sql
     SELECT * FROM `lc_cart_items` WHERE cart_uid = '<cart_uid>';
     ```

**Postconditions**

- очистити кошик шляхом вилучення товару http://localhost/litecart/checkout

<a id="tc-cart-002"></a>
## TC-CART-002 · Кошик: оновлення кількості Green Duck понад залишок (статус Sold Out) відхиляється з повідомленням

| | |
|---|---|
| **Priority** | Medium |
| **Type** | Negative |

**Preconditions**

- гість (не залогінений, нове вікно інкогніто)
- cookies прийнято (кнопка в банері) → оновити сторінку (F5)
- відкритий DevTools → Network
- `<cart_uid>` = значення куки `cart[uid]` (DevTools → Application → Cookies → http://localhost)
- у кошику товар Green Duck (http://localhost/litecart/rubber-ducks-c-1/subcategory-c-2/green-duck-p-2) у кількості 1 шт
- Green Duck (id: 2, quantity: 30, товар без опцій (розмірів), Sold Out Status = Sold Out, змінюється тут: http://localhost/litecart/admin/?app=catalog&doc=edit_product&category_id=1&product_id=2#tab-stock):

  ```sql
  SELECT * FROM `lc_products` WHERE id = 2;
  ```

- у `lc_cart_items` за `<cart_uid>` рівно 1 рядок: `product_id = 2`, `quantity = 1`, `customer_id = 0`:

  ```sql
  SELECT * FROM `lc_cart_items` WHERE cart_uid = '<cart_uid>';
  ```

**Steps / Expected**

1. Відкрити кошик http://localhost/litecart/checkout
   - відображається кошик з товаром Green Duck, є поле кількості зі значенням 1, кнопки Update та Delete
2. Встановити кількість товару 60 та натиснути кнопку Update
   - на сторінці з'являється повідомлення магазину з текстом «There are only 30 remaining products available for purchase»
   - у полі кількості значення 1
   - у БД рядок у `lc_cart_items` не змінився: `product_id = 2`, `quantity = 1`, `customer_id = 0`:

     ```sql
     SELECT * FROM `lc_cart_items` WHERE cart_uid = '<cart_uid>';
     ```

**Джерело Expected:** так відбувається в картці товару при додаванні в кошик кількості, що перевищує залишок.

**Postconditions**

- очистити кошик шляхом вилучення товару http://localhost/litecart/checkout
- якщо змінювався Sold Out Status, встановити попередній: http://localhost/litecart/admin/?app=catalog&doc=edit_product&category_id=1&product_id=2#tab-stock

<a id="чек-лист-кошик"></a>
## Чек-лист «Кошик»

### Додавання

- [ ] Гість додає 1 шт у порожній кошик → в кошику 1 рядок, кількість 1, індикатор на іконці кошика = 1
- [ ] Гість додає той самий товар (1 шт) удруге → в кошику 1 рядок, кількість 2, індикатор на іконці кошика = 2
- [ ] Кількість 0 у формі на картці товару → форма не надсилається, кошик не змінився
- [ ] Кількість -1 через API → відповідь сервера містить alert "Invalid item quantity", кошик не змінився
- [ ] Кількість "abc" через API → відповідь сервера містить alert "Invalid item quantity", кошик не змінився
- [ ] Кількість "abc" у формі на картці товару → кошик не змінився
- [ ] Кількість 2.6 через API → відповідь сервера містить alert "Invalid item quantity", кошик не змінився [A1]
- [ ] Кількість 2.6 у формі на картці товару → товар не додано, покупець бачить повідомлення [A1]

### Зміна кількості (залишок 30, статус Sold Out)

- [ ] Update з 1 до 30 → кількість 30, сума перерахована
- [ ] Update з 1 до 31 → повідомлення "There are only 30 remaining products available for purchase", кількість лишилась 1 (LC-16, див. TC-CART-002)
- [ ] Update з 1 до 2.6 → кількість лишилась 1, покупець бачить повідомлення [A1]
- [ ] Update з 1 до "abc" → кількість лишилась 1
- [ ] Update з 1 до 0 → товар видалено з кошика, інші товари не змінились [A3]

### Видалення

- [ ] Видалити єдиний товар → кошик порожній
- [ ] Видалити один із двох різних товарів → у кошику лишився тільки невидалений товар, його кількість не змінилась

### Гість і акаунт

- [ ] 1 шт в акаунті + 1 шт того самого товару гостем → login → 1 рядок, кількість 2 [A2]
- [ ] 30 шт в акаунті + 30 шт гостем, залишок 30, статус Sold Out → login → у кошику не більше 30 шт [A2]
- [ ] Гість погодився на cookies, додав товар, закрив і відкрив браузер (не інкогніто) → той самий товар і кількість у кошику

### Assumptions / Questions до PO

- [A1] Вимоги до дробової кількості немає. Вважаємо, що вона відхиляється: одиниця pcs має decimals = 0, інші невалідні значення (-1, abc) відхиляються.
  Question: відхиляти чи округлювати? Якщо округлювати, чи повідомляти покупця?
- [A2] Вимоги до злиття кошиків немає. Вважаємо, що кількості сумуються (як при повторному додаванні), але не понад залишок.
  Question: сумувати, брати кошик акаунта чи гостьовий? Якщо сума більша за залишок — обрізати чи показати помилку?
- [A3] Вимоги до кількості 0 у кошику немає. Вважаємо, що 0 = видалення: поле в кошику дозволяє 0 (min="0"), на картці товару — ні (min="1").
  Question: видаляти товар одразу чи просити підтвердження?

# ROUTE_ORDER_NOTE.md


# Ocelot Route Ordering Note

## Rule 1 - Route cụ thể đặt trước route generic < {everything} >

Đúng:

```txt
/orders/Checkout
/orders/{everything}   /generic
````

Sai:

```txt
/orders/{everything}
/orders/Checkout
```

Nếu generic đứng trước:
* route cụ thể sẽ bị nuốt.

---

## Rule 2 - OData route luôn đặt trên

Đúng:

```txt
/api/Staff/odata/{everything}
/api/Staff/{everything}
```

Nếu đặt ngược:

* OData sẽ lỗi routing.

---

## Rule 3 - Generic route luôn đặt cuối group

Ví dụ:

```txt
/orders/Checkout
/orders/Cart/{everything}
/orders/{everything}
```

Trong đó:

```txt
/orders/{everything}
```

luôn là route cuối.

---

## Rule 4 - Priority càng thấp càng ưu tiên

```txt
Priority 0  -> match trước
Priority 5  -> bình thường
Priority 10 -> generic route
```

---

## Rule 5 - Route có nguy cơ conflict dùng Priority thấp hơn

Ví dụ:

```txt
/orders/Checkout
/orders/{everything}
```

=> Checkout nên:

* đứng trên
* priority thấp hơn

---

## Rule 6 - Priority gợi ý cho team

```txt
0  = specific route / checkout / odata
2 = normal route
5 = generic route
```

---

## Rule 7 - Khi thêm route mới

Checklist:

```txt
[ ] Có conflict với route khác không?
[ ] Có route generic nào nuốt route này không?
[ ] Có cần đặt lên trên không?
[ ] Có cần priority thấp hơn không?
[ ] Có phải OData route không?
```

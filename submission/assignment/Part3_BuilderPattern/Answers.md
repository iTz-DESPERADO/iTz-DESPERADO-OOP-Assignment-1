# إجابات الجزء الثالث - Builder Pattern

## Task 3.1 - لماذا constructor يحتوي على حوالي 20 parameter مشكلة؟

وجود constructor بعدد كبير جداً من الـparameters يجعل الاستدعاء صعب القراءة والفهم. عندما تكون عدة قيم من نفس النوع، مثل string أو decimal، يصبح من السهل وضع قيمة في المكان الخطأ من دون أن يكتشف الـcompiler المشكلة.

مثلاً إذا كان الـconstructor يستقبل SubTotal ثم DiscountAmount ثم TaxAmount، فجميعها decimal. تبديل اثنتين منها بالخطأ سيظل كوداً صالحاً من ناحية النوع لكنه يعطي بيانات خاطئة.

المشكلة أيضاً تكبر مع الوقت. إذا أضيفت خاصية اختيارية جديدة، فقد نضطر إلى تعديل الـconstructor وكل أماكن استدعائه أو إنشاء overloads كثيرة يصعب صيانتها.

لكن المشكلة ليست فقط أن الـconstructor طويل. وجود حوالي 20 خاصية غير مترابطة بشكل قوي داخل كلاس واحد يشير إلى مشكلة تصميم أعمق. البيانات تنقسم طبيعياً إلى مجموعات مثل بيانات العميل، عنوان الفاتورة، عنوان الشحن، ومعلومات الطلب والدفع. وجودها كلها كمجموعة مسطحة واحدة يجعل المسؤوليات غير واضحة ويزيد صعوبة التحقق وإعادة الاستخدام.

## Task 3.3 - لماذا الـcomposed builders أفضل من builder واحد كبير؟

### Single Responsibility
AddressBuilder مسؤول فقط عن بناء عنوان صحيح، بينما OrderBuilder مسؤول فقط عن بيانات الطلب والدفع. الـInvoiceBuilder النهائي لا يحتاج إلى معرفة تفاصيل بناء كل مجموعة.

### Independent Validation
AddressBuilder يستطيع التحقق بنفسه من Street وCity وZipCode وCountry قبل إنتاج AddressData. وبذلك لا يحتاج InvoiceBuilder إلى معرفة قواعد صحة العنوان.

OrderBuilder أيضاً يستطيع فرض قواعده الخاصة مثل أن SubTotal وDiscountAmount وTaxAmount لا تكون سالبة، وأن TotalAmount يطابق الحساب المتوقع.

### Reuse
نفس AddressBuilder يستخدم لبناء Billing Address وShipping Address. بدون ذلك سنكرر نفس خصائص ودوال والتحقق من العنوان مرتين داخل builder كبير.

### Readability
في الـcall site تصبح المجموعات واضحة بصرياً: بيانات العميل، ثم عنوان الفاتورة، ثم عنوان الشحن، ثم بيانات الطلب. هذا أسهل في القراءة وأقل عرضة للخطأ من سلسلة طويلة من WithBillingStreet وWithBillingCity وWithShippingStreet وغيرها داخل builder واحد ضخم.

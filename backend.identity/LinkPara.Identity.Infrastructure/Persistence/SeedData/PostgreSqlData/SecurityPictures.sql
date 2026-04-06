INSERT INTO core.security_picture (id, name, bytes, content_type, create_date, update_date, created_by, last_modified_by, record_status)
SELECT gen_random_uuid(), name, convert_to(svg, 'UTF8'), 'image/svg+xml', NOW(), NOW(), 'Seeder', NULL, 'Active'
FROM (VALUES
    ('Elma',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="50" cy="55" r="35" fill="#e74c3c"/><ellipse cx="50" cy="55" rx="20" ry="33" fill="#c0392b" opacity="0.3"/><path d="M50 20 Q55 10 65 12" stroke="#27ae60" stroke-width="3" fill="none" stroke-linecap="round"/><ellipse cx="60" cy="10" rx="8" ry="5" fill="#27ae60" transform="rotate(-15 60 10)"/></svg>'),

    ('Muz',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><path d="M20 70 Q30 20 80 25 Q85 45 60 65 Q40 75 20 70Z" fill="#f1c40f"/><path d="M20 70 Q30 20 80 25" stroke="#e67e22" stroke-width="3" fill="none" stroke-linecap="round"/><path d="M78 24 Q90 22 85 35" stroke="#8e6b00" stroke-width="2.5" fill="none" stroke-linecap="round"/></svg>'),

    ('Araba',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><rect x="10" y="55" width="80" height="25" rx="5" fill="#3498db"/><path d="M25 55 L35 35 L65 35 L75 55Z" fill="#2980b9"/><rect x="35" y="38" width="12" height="14" rx="2" fill="#d6eaf8"/><rect x="53" y="38" width="12" height="14" rx="2" fill="#d6eaf8"/><circle cx="28" cy="82" r="9" fill="#2c3e50"/><circle cx="28" cy="82" r="5" fill="#95a5a6"/><circle cx="72" cy="82" r="9" fill="#2c3e50"/><circle cx="72" cy="82" r="5" fill="#95a5a6"/></svg>'),

    ('Ev',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><polygon points="50,15 10,50 90,50" fill="#e74c3c"/><rect x="18" y="50" width="64" height="40" fill="#ecf0f1"/><rect x="42" y="65" width="16" height="25" fill="#8e44ad"/><rect x="22" y="56" width="14" height="14" fill="#3498db"/><rect x="64" y="56" width="14" height="14" fill="#3498db"/><line x1="29" y1="56" x2="29" y2="70" stroke="#2980b9" stroke-width="1.5"/><line x1="22" y1="63" x2="36" y2="63" stroke="#2980b9" stroke-width="1.5"/><line x1="71" y1="56" x2="71" y2="70" stroke="#2980b9" stroke-width="1.5"/><line x1="64" y1="63" x2="78" y2="63" stroke="#2980b9" stroke-width="1.5"/></svg>'),

    ('Gunes',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="50" cy="50" r="20" fill="#f39c12"/><line x1="50" y1="10" x2="50" y2="22" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="50" y1="78" x2="50" y2="90" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="10" y1="50" x2="22" y2="50" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="78" y1="50" x2="90" y2="50" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="21" y1="21" x2="29" y2="29" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="71" y1="71" x2="79" y2="79" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="79" y1="21" x2="71" y2="29" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/><line x1="21" y1="79" x2="29" y2="71" stroke="#f39c12" stroke-width="4" stroke-linecap="round"/></svg>'),

    ('Agac',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><rect x="44" y="65" width="12" height="25" fill="#7f4f24"/><polygon points="50,10 20,50 80,50" fill="#27ae60"/><polygon points="50,25 18,60 82,60" fill="#2ecc71"/><polygon points="50,38 15,75 85,75" fill="#27ae60"/></svg>'),

    ('Tekne',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><path d="M15 60 Q50 80 85 60 L78 72 Q50 88 22 72Z" fill="#e74c3c"/><rect x="46" y="25" width="4" height="35" fill="#7f4f24"/><polygon points="50,25 50,48 72,38" fill="#f1c40f"/><path d="M5 72 Q20 78 35 72 Q50 66 65 72 Q80 78 95 72" stroke="#3498db" stroke-width="3" fill="none"/></svg>'),

    ('Yildiz',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><polygon points="50,10 61,37 90,37 68,57 76,84 50,66 24,84 32,57 10,37 39,37" fill="#f39c12"/></svg>'),

    ('Cicek',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="50" cy="30" r="12" fill="#e91e8c"/><circle cx="70" cy="50" r="12" fill="#e91e8c"/><circle cx="50" cy="70" r="12" fill="#e91e8c"/><circle cx="30" cy="50" r="12" fill="#e91e8c"/><circle cx="64" cy="36" r="10" fill="#ff69b4"/><circle cx="64" cy="64" r="10" fill="#ff69b4"/><circle cx="36" cy="64" r="10" fill="#ff69b4"/><circle cx="36" cy="36" r="10" fill="#ff69b4"/><circle cx="50" cy="50" r="14" fill="#f1c40f"/><line x1="50" y1="64" x2="50" y2="90" stroke="#27ae60" stroke-width="4" stroke-linecap="round"/></svg>'),

    ('Bisiklet',
     '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="25" cy="68" r="18" fill="none" stroke="#2c3e50" stroke-width="4"/><circle cx="75" cy="68" r="18" fill="none" stroke="#2c3e50" stroke-width="4"/><circle cx="25" cy="68" r="3" fill="#2c3e50"/><circle cx="75" cy="68" r="3" fill="#2c3e50"/><polyline points="25,68 40,40 60,40 75,68" fill="none" stroke="#e74c3c" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"/><polyline points="50,68 60,40" fill="none" stroke="#e74c3c" stroke-width="3.5" stroke-linecap="round"/><polyline points="55,35 65,35" stroke="#2c3e50" stroke-width="4" stroke-linecap="round"/><polyline points="38,40 46,40" stroke="#2c3e50" stroke-width="4" stroke-linecap="round"/></svg>')

) AS t(name, svg)
WHERE NOT EXISTS (SELECT 1 FROM core.security_picture);

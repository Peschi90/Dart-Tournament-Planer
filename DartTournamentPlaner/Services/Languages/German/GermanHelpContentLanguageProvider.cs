using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für ausführliche Hilfe-Inhalte
/// </summary>
public class GermanHelpContentLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Ausführliche Hilfe-Inhalte
            ["HelpGeneralContent"] = "Der Dart Turnier Planer hilft Ihnen bei der Verwaltung von Dart-Turnieren mit bis zu 4 verschiedenen Klassen (Platin, Gold, Silber, Bronze).\n\n" +
                "• Verwenden Sie die Tabs oben, um zwischen den Klassen zu wechseln\n" +
                "• Alle Änderungen werden automatisch gespeichert (wenn aktiviert)\n" +
                "• Die Statusleiste zeigt den aktuellen Speicherstatus an\n" +
                "• Sprache kann in den Einstellungen geändert werden\n\n" +
                "🆕 NEUE FEATURES:\n" +
                "• Lizenz-System für Premium-Features\n" +
                "• API-Integration für externe Anwendungen\n" +
                "• Tournament Hub für Echtzeit-Turnier-Sharing\n" +
                "• Erweiterte Statistiken und Berichterstattung\n" +
                "• Verbesserte Druckfunktionen",

            ["HelpTournamentSetupContent"] = "So richten Sie ein neues Turnier ein:\n\n" +
                "1. Wählen Sie eine Turnierklasse (Platin, Gold, Silber, Bronze)\n" +
                "2. Klicken Sie auf 'Gruppe hinzufügen' um Gruppen zu erstellen\n" +
                "3. Fügen Sie Spieler zu den Gruppen hinzu\n" +
                "4. Konfigurieren Sie die Spielregeln über den 'Regeln konfigurieren' Button\n" +
                "5. Stellen Sie den Modus nach der Gruppenphase ein (Nur Gruppen, Finalrunde, KO-System)\n\n" +
                "Tipp: Mindestens 2 Spieler pro Gruppe sind erforderlich für die Spielgenerierung.",

            ["HelpGroupManagementContent"] = "Gruppenverwaltung:\n\n" +
                "• 'Gruppe hinzufügen': Erstellt eine neue Gruppe\n" +
                "• 'Gruppe entfernen': Löscht die ausgewählte Gruppe (Warnung erscheint)\n" +
                "• 'Spieler hinzufügen': Fügt einen Spieler zur ausgewählten Gruppe hinzu\n" +
                "• 'Spieler entfernen': Entfernt den ausgewählten Spieler\n\n" +
                "Die Spielerliste zeigt alle Spieler der aktuell ausgewählten Gruppe.\n" +
                "Gruppen können beliebig benannt werden und sollten aussagekräftige Namen haben.",

            ["HelpGameRulesContent"] = "Spielregeln konfigurieren:\n\n" +
                "• Spielmodus: 301, 401 oder 501 Punkte\n" +
                "• Finish-Modus: Single Out oder Double Out\n" +
                "• Legs zum Sieg: Anzahl der Legs für einen Sieg\n" +
                "• Mit Sets spielen: Aktiviert das Set-System\n" +
                "• Sets zum Sieg: Anzahl der Sets für einen Turniersieg\n" +
                "• Match-Validierung: Stellt korrekte Ergebniseingabe sicher\n\n" +
                "Alle Regeln gelten für alle Spiele innerhalb einer Turnierklasse.\n" +
                "Verschiedene Klassen können unterschiedliche Regelkonfigurationen haben.",

            ["HelpMatchesContent"] = "Spiele und Ergebnisverwaltung:\n\n" +
                "• Geben Sie Punkte für beide Spieler in jedem Leg ein\n" +
                "• Spiele werden automatisch validiert\n" +
                "• Gewinner werden basierend auf konfigurierten Regeln bestimmt\n" +
                "• Ergebnisse werden sofort gespeichert und synchronisiert\n\n" +
                "FEATURES:\n" +
                "• Echtzeit-Validierung von Spielergebnissen\n" +
                "• Automatisches Voranschreiten zu nächsten Turnierphasen\n" +
                "• Integration mit Tournament Hub für Live-Updates\n" +
                "• API-Endpunkte für externe Ergebniseingabe\n\n" +
                "Spielergebnisse können manuell eingegeben oder von externen Anwendungen über API empfangen werden.",

            ["HelpTournamentPhasesContent"] = "Turnierphasen:\n\n" +
                "1. GRUPPENPHASE:\n" +
                "   • Alle Spieler spielen gegeneinander in ihrer Gruppe\n" +
                "   • Rankings werden basierend auf Siegen/Niederlagen berechnet\n" +
                "   • Punktedifferenz wird für Gleichstände verwendet\n\n" +
                "2. FINALRUNDE:\n" +
                "   • Beste Spieler aus jeder Gruppe qualifizieren sich\n" +
                "   • Einzel-Eliminierungsformat\n" +
                "   • Bestimmt den Gesamtturnier-Gewinner\n\n" +
                "3. KNOCKOUT-SYSTEM:\n" +
                "   • Direktes Eliminierungsturnier\n" +
                "   • Spieler werden nach einem verlorenen Spiel eliminiert\n" +
                "   • Bracket-Style-Progression\n\n" +
                "Phasen-Progression ist automatisch, wenn alle Spiele der aktuellen Phase abgeschlossen sind.",

            ["HelpMenusContent"] = "Menü-Funktionen:\n\n" +
                "DATEI-MENÜ:\n" +
                "• Neu: Neues Turnier erstellen\n" +
                "• Öffnen/Speichern: Turnierdateien laden und speichern\n" +
                "• Drucken: Erweiterte Druckfunktionen mit Lizenz-Features\n\n" +
                "ANSICHT-MENÜ:\n" +
                "• Turnierübersicht: Vollständiger Turnierstatus\n\n" +
                "API-MENÜ:\n" +
                "• API starten/stoppen: REST-API-Server steuern\n" +
                "• API-Dokumentation: API-Dokumentation öffnen\n\n" +
                "TOURNAMENT HUB MENÜ:\n" +
                "• Registrieren/Entregistrieren: Mit Tournament Hub verbinden\n" +
                "• Join-URL anzeigen: Turnier-Zugang teilen\n" +
                "• Manuell synchronisieren: Synchronisation erzwingen\n\n" +
                "LIZENZ-MENÜ:\n" +
                "• Lizenz-Status: Aktuelle Lizenzinformationen anzeigen\n" +
                "• Lizenz aktivieren: Lizenzschlüssel eingeben\n" +
                "• Lizenz kaufen: Neue Lizenz anfragen",

            ["HelpLicenseSystemContent"] = "🔑 LIZENZ-SYSTEM\n\n" +
                "Die Anwendung beinhaltet ein umfassendes Lizenz-System für Premium-Features:\n\n" +
                "CORE FEATURES (Immer kostenlos):\n" +
                "• Basis-Turnierverwaltung\n" +
                "• Spieler- und Gruppenverwaltung\n" +
                "• Spielergebniseingabe\n" +
                "• Basis-Drucken\n" +
                "• Standard-Statistiken\n\n" +
                "PREMIUM FEATURES (Lizenz erforderlich):\n" +
                "• 🌐 API-Integration - REST API für externe Apps\n" +
                "• 🎯 Tournament Hub - Echtzeit-Turnier-Sharing\n" +
                "• 📈 Erweiterte Statistiken - Detaillierte Spieler-Analysen\n" +
                "• 🖨️ Erweiterte Druckfunktionen - Professionelle Turnierberichte\n" +
                "• 📊 Erweiterte Berichterstattung - Export- und Analyse-Tools\n\n" +
                "LIZENZ-VERWALTUNG:\n" +
                "• Lizenz → Lizenz-Status: Aktuelle Lizenzinfos anzeigen\n" +
                "• Lizenz → Lizenz aktivieren: Lizenzschlüssel eingeben\n" +
                "• Lizenz → Lizenz kaufen: Neue Lizenz anfragen\n" +
                "• Hardware-basierte Lizenzierung gewährleistet Sicherheit\n\n" +
                "LIZENZ-TYPEN:\n" +
                "• Personal: Einzelne Computer-Aktivierung\n" +
                "• Professional: Bis zu 5 Computer-Aktivierungen\n" +
                "• Enterprise: Bis zu 10 Computer-Aktivierungen\n" +
                "• Custom: Kontakt für spezielle Anforderungen\n\n" +
                "Ohne Lizenz fällt die Anwendung graceful auf Core-Funktionalität zurück.",

            ["HelpApiIntegrationContent"] = "🌐 API-INTEGRATION\n\n" +
                "Die REST API bietet programmatischen Zugriff auf Turnierdaten:\n\n" +
                "ERSTE SCHRITTE:\n" +
                "1. Stellen Sie sicher, dass Sie eine aktive Lizenz haben\n" +
                "2. Verwenden Sie API → API starten, um den Server zu starten\n" +
                "3. Greifen Sie auf die API-Dokumentation unter der angegebenen URL zu\n" +
                "4. Verwenden Sie API-Endpunkte, um mit Turnierdaten zu interagieren\n\n" +
                "VERFÜGBARE ENDPUNKTE:\n" +
                "• GET /api/tournaments - Alle Turniere auflisten\n" +
                "• GET /api/tournaments/{id} - Spezifisches Turnier abrufen\n" +
                "• GET /api/tournaments/{id}/matches - Turnierspiele abrufen\n" +
                "• POST /api/tournaments/{id}/matches/{matchId}/result - Spielergebnis übermitteln\n" +
                "• GET /api/tournaments/{id}/statistics - Turnierstatistiken abrufen\n\n" +
                "FEATURES:\n" +
                "• Echtzeit-Spielergebnis-Übermittlung\n" +
                "• Live-Turnierdaten-Zugriff\n" +
                "• JSON-basierte RESTful-Schnittstelle\n" +
                "• Automatische Validierung und Fehlerbehandlung\n" +
                "• CORS-Unterstützung für Web-Anwendungen\n\n" +
                "ANWENDUNGSFÄLLE:\n" +
                "• Externe Scoring-Anwendungen\n" +
                "• Turnierwebsites und Displays\n" +
                "• Mobile Begleit-Apps\n" +
                "• Integration mit Dartboard-Systemen\n" +
                "• Individuelle Berichts-Tools\n\n" +
                "Der API-Server läuft lokal und kann von Anwendungen im gleichen Netzwerk erreicht werden.",

            ["HelpTournamentHubContent"] = "🎯 TOURNAMENT HUB\n\n" +
                "Der Tournament Hub ermöglicht Echtzeit-Turnier-Sharing und Zusammenarbeit:\n\n" +
                "SETUP:\n" +
                "1. Stellen Sie sicher, dass Sie eine aktive Lizenz mit Hub-Features haben\n" +
                "2. Tournament Hub → Bei Hub registrieren\n" +
                "3. Teilen Sie die bereitgestellte Join-URL mit Teilnehmern\n" +
                "4. Turnierdaten werden in Echtzeit synchronisiert\n\n" +
                "FEATURES:\n" +
                "• 📡 Echtzeit-WebSocket-Synchronisation\n" +
                "• 📱 Teilbare Turnier-URLs\n" +
                "• 🔄 Automatische Datensynchronisation\n" +
                "• 👥 Multi-Device-Zugriff\n" +
                "• 📊 Live-Turnier-Anzeige\n" +
                "• 🎮 Remote-Spielergebnis-Übermittlung\n\n" +
                "HUB-FUNKTIONEN:\n" +
                "• Bei Hub registrieren: Turnier mit Hub-Server verbinden\n" +
                "• Join-URL anzeigen: Teilbare Turnier-Verbindung erhalten\n" +
                "• Manuell synchronisieren: Synchronisation erzwingen\n" +
                "• Vom Hub entregistrieren: Turnier trennen\n" +
                "• Hub-Einstellungen: Hub-Server-URL konfigurieren\n\n" +
                "STATUS-INDIKATOREN:\n" +
                "• 🟢 Grün: Verbunden und synchronisiert\n" +
                "• 🔴 Rot: Getrennt oder Fehler\n" +
                "• 🟡 Gelb: Verbinde oder Sync läuft\n\n" +
                "VORTEILE:\n" +
                "• Zuschauer können Turnieren live folgen\n" +
                "• Mehrere Offizielle können dasselbe Turnier verwalten\n" +
                "• Remote-Score-Eingabe von mobilen Geräten\n" +
                "• Backup und Redundanz durch Cloud-Sync\n" +
                "• Professionelle Turnierpräsentation\n\n" +
                "Der Hub benötigt eine Internetverbindung und kompatiblen Hub-Server.",

            ["HelpStatisticsContent"] = "📈 STATISTIKEN\n\n" +
                "Erweiterte Statistiken bieten detaillierte Einblicke in Spieler- und Turnierleistung:\n\n" +
                "SPIELER-STATISTIKEN:\n" +
                "• Gesamtzahl gespielter und gewonnener Spiele\n" +
                "• Sieg-Prozentsatz und Leistungstrends\n" +
                "• Durchschnittspunktzahlen und Checkout-Statistiken\n" +
                "• Head-to-Head-Aufzeichnungen\n" +
                "• Leistung nach Turnierklasse\n" +
                "• Verbesserungs-Tracking über Zeit\n\n" +
                "TURNIER-STATISTIKEN:\n" +
                "• Gesamter Turnierfortschritt\n" +
                "• Spiel-Abschlussraten\n" +
                "• Klassenspezifische Leistungsdaten\n" +
                "• Gruppenstände und Rankings\n" +
                "• Phasen-Progressions-Statistiken\n\n" +
                "ERWEITERTE FEATURES (Lizenz erforderlich):\n" +
                "• 📊 Detaillierte Leistungsanalysen\n" +
                "• 📈 Grafische Trendanalyse\n" +
                "• 🎯 Genauigkeits- und Konsistenz-Metriken\n" +
                "• 🏆 Achievement-Tracking\n" +
                "• 📋 Exportierbare Berichte\n" +
                "• 📱 Mobile-freundliche Statistik-Ansichten\n\n" +
                "STATISTIKEN ZUGREIFEN:\n" +
                "• Klicken Sie auf jeden Spieler in der Spielerliste\n" +
                "• Verwenden Sie den Statistiken-Tab (Lizenz erforderlich)\n" +
                "• Anzeige über Turnierübersicht\n" +
                "• Export über API-Endpunkte\n\n" +
                "Statistiken werden in Echtzeit aktualisiert, wenn Spiele abgeschlossen werden und bieten wertvolle Einblicke für Spieler und Turnierorganisatoren.",

            ["HelpPrintingContent"] = "🖨️ DRUCKEN\n\n" +
                "Die Anwendung bietet umfassende Druckfunktionen für Turnierdokumentation:\n\n" +
                "BASIS-DRUCKEN (Immer verfügbar):\n" +
                "• Turnier-Bracket-Übersicht\n" +
                "• Gruppenstände\n" +
                "• Spielpläne\n" +
                "• Basis-Spielerlisten\n\n" +
                "ERWEITERTE DRUCKFUNKTIONEN (Lizenz erforderlich):\n" +
                "• 📄 Professionelle Turnierberichte\n" +
                "• 📊 Statistische Zusammenfassungen\n" +
                "• 🏆 Meisterschaftszertifikate\n" +
                "• 📋 Detaillierte Spielhistorien\n" +
                "• 🎨 Individuelle Formatierung und Branding\n" +
                "• 📱 Mobile-optimierte Drucklayouts\n\n" +
                "DRUCKOPTIONEN:\n" +
                "• Datei → Drucken zum Zugriff auf Druckdialog\n" +
                "• Spezifische Turnierklassen auswählen\n" +
                "• Druckinhalt und -format wählen\n" +
                "• Vorschau vor dem Drucken\n" +
                "• Export zu PDF (Lizenz erforderlich)\n\n" +
                "DRUCKINHALT:\n" +
                "• Turnierübersicht und -struktur\n" +
                "• Vollständige Gruppenstände\n" +
                "• Spielergebnisse und -pläne\n" +
                "• Spielerstatistiken und Rankings\n" +
                "• Turnierregeln und -informationen\n\n" +
                "PROFESSIONELLE FEATURES:\n" +
                "• Individuelle Header und Logos\n" +
                "• Mehrere Formatoptionen\n" +
                "• Batch-Druckfunktionen\n" +
                "• Hochqualitative PDF-Generierung\n" +
                "• Druckjob-Optimierung\n\n" +
                "Erweiterte Druckfeatures benötigen eine aktive Lizenz und bieten professionelle Turnierdokumentation für offizielle Events.",

            ["HelpTipsContent"] = "💡 TIPPS & TRICKS\n\n" +
                "ALLGEMEINE TIPPS:\n" +
                "• Aktivieren Sie Auto-Speichern in den Einstellungen, um Datenverlust zu verhindern\n" +
                "• Verwenden Sie aussagekräftige Gruppen- und Spielernamen\n" +
                "• Überprüfen Sie Turnierregeln vor dem Start von Spielen\n" +
                "• Synchronisieren Sie regelmäßig mit Tournament Hub, falls verbunden\n\n" +
                "EFFIZIENZ-TIPPS:\n" +
                "• Verwenden Sie Tastaturkürzel, wo verfügbar\n" +
                "• Batch-Hinzufügen von Spielern über Copy-Paste\n" +
                "• Richten Sie Vorlagen für wiederkehrende Turniere ein\n" +
                "• Verwenden Sie die Turnierübersicht für schnelle Status-Checks\n\n" +
                "ERWEITERTE FEATURES:\n" +
                "• Drücken Sie Shift+Klick auf Drucken für Debug-Informationen\n" +
                "• Klicken Sie auf Hub-Status-Indikator für Debug-Konsole\n" +
                "• Verwenden Sie API-Integration für automatisierte Punktevergabe\n" +
                "• Nutzen Sie Tournament Hub für Remote-Management\n\n" +
                "PROBLEMLÖSUNG:\n" +
                "• Überprüfen Sie Internetverbindung für Hub/API-Features\n" +
                "• Verifizieren Sie Lizenz-Status für Premium-Features\n" +
                "• Verwenden Sie Hilfe → Fehler melden für Probleme\n" +
                "• Überprüfen Sie die Debug-Konsole für technische Informationen\n\n" +
                "LIZENZ-OPTIMIERUNG:\n" +
                "• Erwägen Sie Professional-Lizenz für mehrere Computer\n" +
                "• Verwenden Sie Enterprise-Lizenz für Turnierorganisationen\n" +
                "• Kontaktieren Sie Support für individuelle Lizenzanforderungen\n" +
                "• Premium-Features verbessern Turnierverwaltung erheblich\n\n" +
                "SUPPORT:\n" +
                "• Verwenden Sie Hilfe → Fehler melden, um Probleme zu melden\n" +
                "• Fügen Sie Systeminformationen in Fehlerberichte ein\n" +
                "• Überprüfen Sie das GitHub-Repository für Updates\n" +
                "• Erwägen Sie, die Entwicklung durch Spenden zu unterstützen"
        };
    }
}
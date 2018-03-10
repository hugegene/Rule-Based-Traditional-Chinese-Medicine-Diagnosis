
;;;======================================================
;;;   Wine Expert Sample Problem
;;;
;;;     WINEX: The WINe EXpert system.
;;;     This example selects an appropriate wine
;;;     to drink with a meal.
;;;
;;;     CLIPS Version 6.3 Example
;;;
;;;     For use with the CLIPSJNI
;;;======================================================

(defmodule MAIN (export ?ALL))

;;*****************
;;* MAIN MODULE *
;;*****************

(deftemplate MAIN::text-for-id
   (slot id)
   (slot text))

(deftemplate MAIN::welcome
  (slot message))

(deftemplate MAIN::top
  (slot top))

(deftemplate MAIN::question
  (slot qn)
  (slot top))

(deftemplate MAIN::UI-state
   (slot id (default-dynamic (gensym*)))
   (slot display)
   (slot relation-asserted (default none))
   (slot response (default none))
   (multislot valid-answers)
   (multislot display-answers)
   (slot state (default interview)))

(deftemplate MAIN::attribute
   (slot name)
   (slot value)
   (slot certainty (default 100.0)))

    ;;-----------------------------------------------------------------------------------------------------------

(defmethod handle-state ((?state SYMBOL (eq ?state greeting))
                         (?message LEXEME)
                         (?relation-asserted SYMBOL)
                         (?valid-answers MULTIFIELD))
   (assert (UI-state (display ?message)
                     (relation-asserted greeting)
                     (valid-answers yes)
                     (display-answers yes)
					 (state ?state)))
   (halt))

   (defmethod handle-state ((?state SYMBOL (eq ?state interview))
                         (?message LEXEME)
                         (?relation-asserted SYMBOL)
                         (?response PRIMITIVE)
                         (?valid-answers MULTIFIELD)
                         (?display-answers MULTIFIELD))
   (assert (UI-state (display ?message)
                     (relation-asserted ?relation-asserted)
                     (state ?state)
                     (response ?response)
                     (valid-answers ?valid-answers)
                     (display-answers ?display-answers)))
   (halt))


(defmethod handle-state ((?state SYMBOL (eq ?state conclusion))
                         (?display LEXEME))
   (assert (UI-state (display ?display)
                     (state ?state)
                     (valid-answers)
                     (display-answers)))
   (halt))


(deffunction MAIN::find-text-for-id (?id)
   ;; Search for the text-for-id fact
   ;; with the same id as ?id
   (bind ?fact
      (find-fact ((?f text-for-id))
                  (eq ?f:id ?id)))
   (if ?fact
      then
      (fact-slot-value (nth$ 1 ?fact) text)
      else
      ?id))


   ;;-----------------------------------------------------------------------------------------------------------

(defrule MAIN::start
  (declare (salience 10000)
           (auto-focus TRUE))
  =>
  (set-fact-duplication TRUE)
  (focus CHOOSE-QUALITIES BODY))


(defrule MAIN::combine-certainties
  ?rem1 <- (attribute (name ?rel) (value ?val) (certainty ?per1))
  ?rem2 <- (attribute (name ?rel) (value ?val) (certainty ?per2))
  (test (neq ?rem1 ?rem2))
  =>
  (retract ?rem1)
  (modify ?rem2 (certainty (* (+ (/ ?per1 100) (* (/ ?per2 100) (- 1 (/ ?per1 100)))) 100))))


  ;;;;(modify ?rem2 (certainty (/ (- (* 100 (+ ?per1 ?per2)) (* ?per1 ?per2)) 100))))

  
(defrule MAIN::start-interview
  (welcome (message ?message-id))
  (not (attribute (name greeting)))
  =>
  (handle-state greeting
                (find-text-for-id ?message-id)
                greeting
                (create$)))

(defrule MAIN::no-more-interview
  (not (and (top (top ?t)) (question (qn ?qn) (top ?t))))
  =>
  (handle-state conclusion
                (find-text-for-id conclusion)))

(defrule MAIN::continue-interview
  (question (qn ?qn) (top ?t))
  (top (top ?t))
  (not(attribute(name ?qn)))
  =>
  (handle-state interview
                (find-text-for-id ?qn)
                ?qn
				no
				(create$ yes no)
                (create$ yes no))) 
				



;;******************
;; *RULES MODULE*
;;******************

(defmodule RULES (import MAIN ?ALL) (export ?ALL))

(deftemplate RULES::rule
  (multislot if)
  (multislot then))

(defrule RULES::thow-away-questions
  ?f <- (question(qn ?qn))
  (attribute(name ?qn))
  =>
  (retract ?f))

(defrule RULES::throw-away-ands-in-antecedent
  ?f <- (rule (if and $?rest))
  =>
  (modify ?f (if ?rest)))

(defrule RULES::throw-away-ands-in-consequent
  ?f <- (rule (if)(then and $?rest))
  =>
  (modify ?f (then ?rest)))

(defrule RULES::remove-is-condition-when-satisfied
  ?f <- (rule (if ?attribute is ?value $?rest))
  (attribute (name ?attribute) 
             (value ?value))
  =>
  (modify ?f (if ?rest)))

;(defrule RULES::remove-is-not-condition-when-satisfied
 ; ?f <- (rule (certainty ?c1) 
          ;    (if ?attribute is-not ?value $?rest))
  ;(attribute (name ?attribute) (value ~?value) (certainty ?c2))
  ;=>
  ;(modify ?f (certainty (min ?c1 ?c2)) (if ?rest)))


(defrule RULES::perform-rule-consequent-with-certainty
  ?f <- (rule (if) 
              (then ?attribute is ?value with certainty ?c2 $?rest))
  =>
  (modify ?f (then ?rest))
  (assert (attribute (name ?attribute) 
                     (value ?value)
                     (certainty ?c2))))


;(defrule RULES::perform-rule-consequent-without-certainty
 ; ?f <- (rule (certainty ?c1)
              ;(if)
             ; (then ?attribute is ?value $?rest))
  ;(test (or (eq (length$ ?rest) 0)
            ;(neq (nth$ 1 ?rest) with)))
 ; =>
  ;(modify ?f (then ?rest))
  ;(assert (attribute (name ?attribute) (value ?value) (certainty ?c1))))

;;*******************************
;;*CHOOSE-QUALITIES MODULE*
;;*******************************

(defmodule CHOOSE-QUALITIES (import RULES ?ALL)
                            (import MAIN ?ALL)
							(export ?ALL))

(defrule CHOOSE-QUALITIES::startit => (focus RULES))

(deffacts the-body-rules

   (welcome (message WelcomeMessage))

   (text-for-id
   (id WelcomeMessage)
   (text "Welcome to the TCM Recomendation Expert System."))

   (text-for-id
   (id conclusion)
   (text "Please see our conclusion on the right."))

  ; Rules for picking the best body


(rule (if tongue-condition is thin-white)
        (then body-type is qi-stagnation with certainty 40 and
			body-type is qi-deficient with certainty 20 and
			body-type is yang-deficient with certainty 30 and
			body-type is damp-retention with certainty 20 and
			body-type is blood-deficient with certainty 20))

(rule (if tongue-condition is red-spot)
        (then body-type is qi-deficient with certainty 40 and
			body-type is qi-stagnation with certainty 30 and
			body-type is yang-deficient with certainty 30 and
			body-type is damp-retention with certainty 20 and
			body-type is blood-deficient with certainty 20))

(rule (if tongue-condition is yellow-greasy)
        (then body-type is damp-heat with certainty 40 and
			body-type is yin-deficient with certainty 20 and
			body-type is heat with certainty 30))

(rule (if tongue-condition is thin-yellow)
        (then body-type is heat with certainty 40 and
			body-type is damp-heat with certainty 30 and
			body-type is yin-deficient with certainty 20))

(rule (if tongue-condition is thick-white)
        (then body-type is yang-deficient with certainty 40 and
			body-type is qi-deficient with certainty 30 and
			body-type is qi-stagnation with certainty 20 and
			body-type is damp-retention with certainty 20 and
			body-type is blood-deficient with certainty 20))

(rule (if tongue-condition is white-greasy)
        (then body-type is damp-retention with certainty 40 and
			body-type is qi-deficient with certainty 20 and
			body-type is qi-stagnation with certainty 20 and
			body-type is yang-deficient with certainty 30 and
			body-type is blood-deficient with certainty 20))

(rule (if tongue-condition is cracks)
        (then body-type is yin-deficient with certainty 40 and
			body-type is damp-heat with certainty 20 and
			body-type is heat with certainty 30))

(rule (if tongue-condition is black-purple)
        (then body-type is blood-stasis with certainty 40 and
		body-type is yin-deficient with certainty 20))

(rule (if tongue-condition is pale)
        (then body-type is blood-deficient with certainty 40 and
			body-type is qi-deficient with certainty 30 and
			body-type is qi-stagnation with certainty 20 and
			body-type is yang-deficient with certainty 20 and
			body-type is damp-retention with certainty 20))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	;;; PROFILING RULES
	; profile smoking or drinking, damp-heat cf 40
  
  (rule (if profile-smoking is yes) 
        (then body-type is damp-heat with certainty 45 ))

	; profile stress, qi-stagnation cf 40
  (rule (if profile-stress is yes) 
        (then body-type is qi-stagnation with certainty 25 ))

	; profile high blood pressure yin-deficient cf 40
  (rule (if profile-highblood is yes) 
        (then body-type is yin-deficient with certainty 55 ))

	; profile diabetes yin-deficient cf 40
  (rule (if profile-diabetes is yes) 
        (then body-type is yin-deficient with certainty 55 ))

	; profile poor sleep yin-deficient cf 40
  (rule (if profile-poorsleep is yes) 
        (then body-type is yin-deficient with certainty 20 ))

	; profile heart disease qi-deficient cf 40
  (rule (if profile-heartdisease is yes) 
        (then body-type is qi-deficient with certainty 55 ))

	; profile overweight damp-retention cf 40
  (rule (if profile-overweight is yes) 
        (then body-type is damp-retention with certainty 40 ))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
	;;; JUMP QUESTIONS RULES for TEXT-FOR-ID
 
 
  (rule (if constip is yes) 
        (then body-type is heat with certainty 40))
  (rule (if irritable is yes) 
        (then body-type is yin-deficient with certainty 40))
  (rule (if sticky-clammy is yes) 
        (then body-type is damp-heat with certainty 40 ))
(rule (if bad-breath is yes) 
        (then body-type is damp-heat with certainty 40))
(rule (if feverish is yes) 
        (then body-type is heat with certainty 40))
(rule (if bitter-taste is yes) 
        (then body-type is heat with certainty 40))		
(rule (if poor-appetite is yes) 
        (then body-type is qi-deficient with certainty 40))
(rule (if fatigue is yes) 
        (then body-type is blood-deficient with certainty 40))
(rule (if depression is yes) 
        (then body-type is qi-stagnation with certainty 40))
(rule (if palpitations is yes) 
        (then body-type is blood-deficient with certainty 40))
(rule (if sweating is yes) 
        (then body-type is qi-deficient with certainty 40))
(rule (if insomnia is yes) 
        (then body-type is qi-stagnation with certainty 40))
 (rule (if ringing-ears is yes) 
        (then body-type is yin-deficient with certainty 40))

  (rule (if bloated is yes) 
        (then body-type is damp-retention with certainty 40 ))
  (rule (if dizzy is yes) 
        (then body-type is damp-retention with certainty 40 ))
  (rule (if pale-lips is yes) 
        (then body-type is yang-deficient with certainty 40 ))
  (rule (if cold-easily is yes) 
        (then body-type is yang-deficient with certainty 40))
  (rule (if cold-limbs is yes) 
        (then body-type is blood-stasis with certainty 40))
  (rule (if head-aches is yes) 
        (then body-type is blood-stasis with certainty 40))





;;;;;;; TOP PROFILE questions
	
	(question (qn constip) (top heat))
	(question (qn irritable) (top yin-deficient))
	(question (qn sticky-clammy) (top damp-heat))
	(question (qn bad-breath) (top damp-heat))
	(question (qn feverish) (top heat))
	(question (qn bitter-taste) (top heat))


	(question (qn poor-appetite) (top qi-deficient))
	(question (qn fatigue) (top blood-deficient))
	(question (qn depression) (top qi-stagnation))
	(question (qn palpitations) (top blood-deficient))
	(question (qn sweating) (top qi-deficient))
	(question (qn insomnia) (top qi-stagnation))
	(question (qn ringing-ears) (top yin-deficient))


	(question (qn bloated) (top damp-retention))
	(question (qn dizzy) (top damp-retention))
	(question (qn pale-lips) (top yang-deficient))
	(question (qn cold-easily) (top yang-deficient))
	(question (qn cold-limbs) (top blood-stasis))
	(question (qn head-aches) (top blood-stasis))




;;;;;;;; TEXT-FOR-ID
    ; text-for-id top damp-retention
    (text-for-id (id bloated)
        (text "Do you have bloated stomach?"))

    (text-for-id (id dizzy)
        (text "Are you feeling lightheaded or dizzy?"))

    (text-for-id (id pale-lips)
        (text "Do you have pale lips?"))

    ; text-for-id top damp-heat
    (text-for-id (id sticky-clammy)
        (text "Do you have sticky or clammy skin?"))

    ; text-for-id top blood-stasis and heat
    (text-for-id (id head-aches)
        (text "Are you experiencing headaches?"))

    ; text-for-id top damp-heat and heat
    (text-for-id (id bad-breath)
        (text "Do you notice bad breath and breath odours?"))
    (text-for-id (id bitter-taste)
        (text "Do you have bitter taste in mouth?"))

    ; text-for-id top qi-stagnation
    (text-for-id (id depression)
        (text "Are you feeling depressed?"))

    ; text-for-id top qi-stagnation and blood-deficient
    (text-for-id (id insomnia)
        (text "Do you have insomnia?"))
    (text-for-id (id palpitations)
        (text "Do you notice fast heart beats or palpitations in your chest?"))

    ; text-for-id top heat
    (text-for-id (id constip)
        (text "Are you experiencing constipation?"))

    ; text-for-id top yin-deficient and heat
    (text-for-id (id irritable)
        (text "Do you often feel irritable for no reason?"))

    (text-for-id (id feverish)
        (text "Are you feeling a bit feverish?"))

    ; text-for-id top yin-deficient 
    (text-for-id (id ringing-ears)
        (text "Do you keep hearing ringing, buzzing or hissing sounds that appear to come from inside your body?"))

    ; text-for-id top qi-deficient and blood-deficient 
    (text-for-id (id fatigue)
        (text "Do you feel fatigue or exhaustion?"))
    (text-for-id (id poor-appetite)
        (text "Do you have decreased or poor appetite?"))

    ; text-for-id top qi-deficient and damp-retention 
    (text-for-id (id sweating)
        (text "Do you sweat spontaneously or excessively?"))

    ; text-for-id top qi-deficient, yang-deficient, blood-stasis
    (text-for-id (id cold-easily)
        (text "Do you feel cold easily?"))

    ; text-for-id top yang-deficient, blood-stasis
    (text-for-id (id cold-limbs)
        (text "Are your hands and feet cold?"))

    ; text-for-id top damp-heat, blood-stasis
    (text-for-id (id acne-pimples)
        (text "Is your skin prone to acne or pimples?"))


)

;;************************
;;*BODY MODULE *
;;************************

(defmodule BODY (import MAIN ?ALL)
				(import RULES ?ALL)
				(import CHOOSE-QUALITIES ?ALL)
                (export deffunction get-body-list))

(deffunction BODY::body-sort (?w1 ?w2)
   (< (fact-slot-value ?w1 certainty)
      (fact-slot-value ?w2 certainty)))

(deffunction BODY::get-body-list ()
  (bind ?facts (find-all-facts ((?f attribute))
                               (eq ?f:name body-type)))
                                    
  (sort body-sort ?facts)
  )


  


  
  



  


# Interview Preparation

Every module in this repository ends with a **🎤 Interview Questions** section (Junior / Mid / Senior, each with an expected answer and a common trap). This directory indexes them by level and adds scenario-based questions that test *reasoning*, not memorized definitions.

## How to use this
1. Don't just read the expected answers — explain them out loud, unprompted, before checking.
2. For "Senior" questions especially, practice explaining the **trade-off**, not just the "correct" choice — interviewers are usually testing judgment, not a single right answer.
3. Work through the scenario questions below without looking at the linked module first; then check your answer against it.

## Index by Level

### Junior-level topics (definitions & basic usage)
[01](../01-CSharp-Fundamentals) · [02](../02-Data-Types) · [03](../03-Value-vs-Reference-Types) · [04](../04-Control-Flow) · [05](../05-Methods) · [06](../06-OOP) · [07](../07-Interfaces) · [09](../09-Delegates-Events) · [10](../10-LINQ) · [11](../11-Exception-Handling) · [18](../18-Dependency-Injection) · [19](../19-Web-API) · [22](../22-Authentication)

### Mid-level topics (practical implementation & "why")
[08](../08-Advanced-CSharp) · [12](../12-Async-Await) · [13](../13-Memory-GC) · [14](../14-Modern-CSharp) · [16](../16-ASPNet-Core) · [17](../17-Middleware) · [20](../20-SQL-Server) · [21](../21-Entity-Framework-Core) · [23](../23-Security) · [24](../24-Design-Patterns) · [25](../25-SOLID) · [29](../29-Caching) · [30](../30-Background-Services) · [34](../34-Testing) · [37](../37-Docker) · [38](../38-CI-CD)

### Senior-level topics (architecture, internals, trade-offs)
[26](../26-Clean-Architecture) · [27](../27-DDD) · [28](../28-Architecture) · [31](../31-Messaging) · [32](../32-Microservices) · [33](../33-Resilience) · [35](../35-Observability) · [36](../36-Performance) · [39](../39-Azure)

## Scenario-Based Questions (cross-cutting)

These deliberately span multiple modules — real interviews rarely ask about one isolated concept.

1. **"Our API is slow under load, but CPU usage is low."** What do you check first, and in what order? *(Touches: [12](../12-Async-Await), [21](../21-Entity-Framework-Core), [36](../36-Performance))*

2. **"A production incident: memory grows steadily over days until an OOM restart."** Walk through your diagnostic process. *(Touches: [09](../09-Delegates-Events), [13](../13-Memory-GC), [29](../29-Caching))*

3. **"Design the service boundaries for an e-commerce platform."** What informs where you draw the lines? *(Touches: [27](../27-DDD), [28](../28-Architecture), [32](../32-Microservices))*

4. **"A junior developer asks why we need both DTOs and domain entities — isn't that duplicate code?"** How do you explain the reasoning, not just the rule? *(Touches: [19](../19-Web-API), [26](../26-Clean-Architecture))*

5. **"How would you add a new payment provider to an existing system without risking regressions in the existing ones?"** *(Touches: [06](../06-OOP), [24](../24-Design-Patterns), [25](../25-SOLID))*

6. **"A downstream service is intermittently slow. How do you stop it from taking down your whole application?"** *(Touches: [12](../12-Async-Await), [33](../33-Resilience))*

7. **"Explain, end to end, what happens between a client sending an HTTP request and your controller's code running."** *(Touches: [01](../01-CSharp-Fundamentals), [16](../16-ASPNet-Core), [17](../17-Middleware), [18](../18-Dependency-Injection))*

8. **"When would you choose a modular monolith over microservices for a new project, and how would you keep the door open to split later?"** *(Touches: [27](../27-DDD), [28](../28-Architecture), [32](../32-Microservices))*

## Tips for the interview itself
- If asked "what's the difference between X and Y," always follow with **when you'd choose each** — this is what separates a Junior answer from a Senior one.
- If you don't know something, say so and reason about it from first principles rather than guessing confidently — interviewers consistently rate this higher than a bluffed wrong answer.
- For system design questions, always state your assumptions out loud before diving in (scale, team size, consistency requirements) — see [28 — Architecture](../28-Architecture) for the vocabulary to do this well.
